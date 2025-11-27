using BookVectorMVC.Services.Interfaces;
using BookVectorMVC.Models;
using BookVectorMVC.Data;
using System.Text.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BookVectorMVC.Services
{
    public class BookRecommendationService : IBookRecommendationService
    {
        private readonly BookDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookRecommendationService> _logger;

        public BookRecommendationService(BookDbContext context, HttpClient httpClient, ILogger<BookRecommendationService> logger)
        {
            _context = context;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Book>> GetPersonalizedRecommendations(string userId, int count = 5)
        {
            try
            {
                // 獲取使用者的閱讀歷史和偏好
                var userPreferences = await AnalyzeUserPreferences(userId);
                
                // 使用AI分析推薦相似書籍
                var recommendedBooks = await GetAIRecommendations(userPreferences, count);
                
                return recommendedBooks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個人化推薦時發生錯誤");
                return await GetFallbackRecommendations(count);
            }
        }

        public async Task<List<Book>> GetSimilarBooks(int bookId, int count = 5)
        {
            try
            {
                var targetBook = await _context.Books.FindAsync(bookId);
                if (targetBook == null) return new List<Book>();

                var prompt = $@"基於以下書籍資訊，請推薦{count}本相似的書籍：

目標書籍：
- 標題：{targetBook.Title}
- 描述：{targetBook.Description}

請推薦在主題、風格、或內容上相似的書籍，並說明推薦理由。";

                var aiResponse = await CallPerplexityAPI(prompt, "書籍推薦助手");
                return await ParseBookRecommendations(aiResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取相似書籍時發生錯誤");
                return await GetRandomBooks(count);
            }
        }

        public async Task<List<string>> GenerateBookTags(string description)
        {
            try
            {
                var prompt = $@"請為以下書籍描述生成5-8個相關標籤：

描述：{description}

請生成能準確描述書籍主題、類型、特色的標籤。
以JSON格式回答：{{""tags"": [""標籤1"", ""標籤2"", ...]}}";

                var response = await CallPerplexityAPI(prompt, "標籤生成器");
                return ParseTagsResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成書籍標籤時發生錯誤");
                return GetDefaultTags();
            }
        }

        public async Task<string> ClassifyBookCategory(string title, string description)
        {
            try
            {
                var prompt = $@"請將以下書籍分類到最合適的類別：

書名：{title}
描述：{description}

請從以下類別中選擇最合適的：
文學小說、科學技術、歷史傳記、商業管理、教育學習、
藝術設計、健康生活、哲學宗教、法律政治、其他

只需回答類別名稱。";

                var response = await CallPerplexityAPI(prompt, "書籍分類器");
                return ExtractCategory(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分類書籍時發生錯誤");
                return "其他";
            }
        }

        public async Task<string> GenerateBookSummary(string title, string description)
        {
            try
            {
                var prompt = $@"請為以下書籍生成一個簡潔的摘要（100-150字）：

書名：{title}
描述：{description}

請突出書籍的核心價值、主要內容和適讀對象。";

                return await CallPerplexityAPI(prompt, "摘要生成器");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成書籍摘要時發生錯誤");
                return "暫無摘要資訊。";
            }
        }

        public async Task<string> AnswerLibraryQuestion(string question)
        {
            try
            {
                // 獲取圖書館中的相關書籍
                var relevantBooks = await GetRelevantBooksForQuestion(question);
                
                var booksContext = string.Join("\n", relevantBooks.Take(5).Select(b => 
                    $"《{b.Title}》- {b.Description?.Substring(0, Math.Min(100, b.Description?.Length ?? 0))}..."));

                var prompt = $@"基於以下圖書館藏書資訊回答問題：

圖書館藏書：
{booksContext}

使用者問題：{question}

請基於圖書館的實際藏書提供準確、有用的回答。如果圖書館沒有相關書籍，請誠實告知並建議可能的替代方案。";

                return await CallPerplexityAPI(prompt, "圖書館助手");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回答圖書館問題時發生錯誤");
                return "抱歉，目前無法回答您的問題。請稍後再試或聯繫圖書館管理員。";
            }
        }

        private async Task<string> AnalyzeUserPreferences(string userId)
        {
            // 簡化版：基於用戶操作記錄分析偏好
            // 實際應用中可以追蹤用戶的搜尋歷史、借閱記錄等
            return "使用者偏好：科技、程式設計、商業管理類書籍";
        }

        private async Task<List<Book>> GetAIRecommendations(string preferences, int count)
        {
            var prompt = $@"基於使用者偏好，推薦{count}本書籍：

使用者偏好：{preferences}

請推薦書名、作者和簡短理由。";

            var response = await CallPerplexityAPI(prompt, "個人化推薦");
            return await ParseBookRecommendations(response);
        }

        private async Task<string> CallPerplexityAPI(string prompt, string systemRole)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    return "API未配置，無法提供AI功能。";
                }

                var requestBody = new
                {
                    model = "llama-3.1-sonar-small-128k-online",
                    messages = new[]
                    {
                        new { role = "system", content = $"你是一個{systemRole}，請提供準確且有用的回答。" },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 1000,
                    temperature = 0.3
                };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var response = await _httpClient.PostAsync("https://api.perplexity.ai/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return "API呼叫失敗。";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonDocument.Parse(responseContent);
                
                if (apiResponse.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    return choices[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                }

                return "無法獲得回應。";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "呼叫Perplexity API時發生錯誤");
                return "AI服務暫時不可用。";
            }
        }

        private async Task<List<Book>> GetRelevantBooksForQuestion(string question)
        {
            // 簡化版：基於關鍵字搜尋相關書籍
            var keywords = ExtractKeywords(question);
            var query = _context.Books.AsQueryable();

            foreach (var keyword in keywords)
            {
                query = query.Where(b => b.Title.Contains(keyword) || 
                                        (b.Description != null && b.Description.Contains(keyword)));
            }

            return await query.Take(10).ToListAsync();
        }

        private List<string> ExtractKeywords(string text)
        {
            // 簡化版關鍵字提取
            var commonWords = new[] { "的", "是", "在", "有", "和", "或", "但", "如果", "因為" };
            return text.Split(' ', '，', '。', '？', '！')
                      .Where(w => w.Length > 1 && !commonWords.Contains(w))
                      .Take(5)
                      .ToList();
        }

        private async Task<List<Book>> ParseBookRecommendations(string response)
        {
            // 簡化版：返回隨機書籍作為示例
            return await GetRandomBooks(3);
        }

        private List<string> ParseTagsResponse(string response)
        {
            try
            {
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                
                if (jsonStart != -1 && jsonEnd != -1)
                {
                    var jsonContent = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                    var doc = JsonDocument.Parse(jsonContent);
                    
                    if (doc.RootElement.TryGetProperty("tags", out var tagsArray))
                    {
                        return tagsArray.EnumerateArray()
                                      .Select(tag => tag.GetString() ?? "")
                                      .Where(tag => !string.IsNullOrEmpty(tag))
                                      .ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析標籤回應時發生錯誤");
            }

            return GetDefaultTags();
        }

        private string ExtractCategory(string response)
        {
            var categories = new[] { "文學小說", "科學技術", "歷史傳記", "商業管理", "教育學習",
                                   "藝術設計", "健康生活", "哲學宗教", "法律政治" };
            
            foreach (var category in categories)
            {
                if (response.Contains(category))
                {
                    return category;
                }
            }
            
            return "其他";
        }

        private async Task<List<Book>> GetRandomBooks(int count)
        {
            return await _context.Books
                               .OrderBy(b => Guid.NewGuid())
                               .Take(count)
                               .ToListAsync();
        }

        private async Task<List<Book>> GetFallbackRecommendations(int count)
        {
            return await _context.Books
                               .OrderByDescending(b => b.BookId)
                               .Take(count)
                               .ToListAsync();
        }

        private List<string> GetDefaultTags()
        {
            return new List<string> { "一般圖書", "推薦閱讀" };
        }
    }
}