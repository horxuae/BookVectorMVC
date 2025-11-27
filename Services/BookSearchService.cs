using BookVectorMVC.Services.Interfaces;
using BookVectorMVC.Controllers;
using System.Text.Json;
using System.Text;

namespace BookVectorMVC.Services
{
    public class BookSearchService : IBookSearchService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookSearchService> _logger;

        public BookSearchService(HttpClient httpClient, ILogger<BookSearchService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<BookSearchResult>> SearchOnlineBooks(string query)
        {
            try
            {
                // 首先嘗試使用 Perplexity API 進行智能搜尋
                var perplexityResults = await SearchWithPerplexity(query);
                if (perplexityResults.Any())
                {
                    return perplexityResults;
                }

                // 如果 Perplexity 失敗，回退到 Google Books API
                return await SearchWithGoogleBooks(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋線上書籍時發生錯誤");
                
                // 如果所有API都失敗，返回模擬的搜尋結果
                return GetMockSearchResults(query);
            }
        }

        private async Task<List<BookSearchResult>> SearchWithPerplexity(string query)
        {
            try
            {
                var perplexityApiKey = Environment.GetEnvironmentVariable("PERPLEXITY_API_KEY");
                if (string.IsNullOrEmpty(perplexityApiKey))
                {
                    _logger.LogWarning("未設置 PERPLEXITY_API_KEY 環境變數，跳過 Perplexity 搜尋");
                    return new List<BookSearchResult>();
                }

                var prompt = GenerateSearchPrompt(query);

                // 可配置的搜尋參數
                var model = Environment.GetEnvironmentVariable("PERPLEXITY_MODEL") ?? "llama-3.1-sonar-small-128k-online";
                var maxTokens = int.Parse(Environment.GetEnvironmentVariable("PERPLEXITY_MAX_TOKENS") ?? "2000");
                var temperature = double.Parse(Environment.GetEnvironmentVariable("PERPLEXITY_TEMPERATURE") ?? "0.2");
                var topP = double.Parse(Environment.GetEnvironmentVariable("PERPLEXITY_TOP_P") ?? "0.9");
                
                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "system", content = GetSystemPrompt() },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = maxTokens,
                    temperature = temperature,
                    top_p = topP,
                    return_citations = true,
                    search_domain_filter = GetSearchDomains(),
                    search_recency_filter = "month" // 搜尋最近一個月的資料
                };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                });

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {perplexityApiKey}");

                var response = await _httpClient.PostAsync("https://api.perplexity.ai/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Perplexity API 呼叫失敗: {response.StatusCode}");
                    return new List<BookSearchResult>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var perplexityResponse = JsonDocument.Parse(responseContent);
                
                if (perplexityResponse.RootElement.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    var messageContent = choices[0].GetProperty("message").GetProperty("content").GetString();
                    
                    // 嘗試解析 JSON 回應
                    return ParsePerplexityResponse(messageContent);
                }

                return new List<BookSearchResult>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Perplexity API 搜尋失敗");
                return new List<BookSearchResult>();
            }
        }

        private List<BookSearchResult> ParsePerplexityResponse(string content)
        {
            try
            {
                // 嘗試提取 JSON 部分
                var jsonStart = content.IndexOf('{');
                var jsonEnd = content.LastIndexOf('}');
                
                if (jsonStart == -1 || jsonEnd == -1) return new List<BookSearchResult>();
                
                var jsonContent = content.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var doc = JsonDocument.Parse(jsonContent);
                
                var books = new List<BookSearchResult>();
                
                if (doc.RootElement.TryGetProperty("books", out var booksArray))
                {
                    foreach (var book in booksArray.EnumerateArray())
                    {
                        var result = new BookSearchResult
                        {
                            Title = book.TryGetProperty("title", out var title) ? title.GetString() ?? "" : "",
                            Author = book.TryGetProperty("author", out var author) ? author.GetString() ?? "未知作者" : "未知作者",
                            Description = book.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                            PublishYear = book.TryGetProperty("publishYear", out var year) ? year.GetString() ?? "" : "",
                            ISBN = book.TryGetProperty("isbn", out var isbn) ? isbn.GetString() ?? "" : "",
                            CoverImage = "" // Perplexity 通常不提供封面圖片
                        };
                        
                        if (!string.IsNullOrEmpty(result.Title))
                        {
                            books.Add(result);
                        }
                    }
                }
                
                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析 Perplexity 回應時發生錯誤");
                return new List<BookSearchResult>();
            }
        }

        private async Task<List<BookSearchResult>> SearchWithGoogleBooks(string query)
        {
            try
            {
                // 使用Google Books API進行搜尋
                var encodedQuery = Uri.EscapeDataString(query);
                var apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={encodedQuery}&maxResults=10&langRestrict=zh";
                
                // 重置 HttpClient headers（因為之前可能設置了 Perplexity 的 headers）
                _httpClient.DefaultRequestHeaders.Clear();
                
                var response = await _httpClient.GetStringAsync(apiUrl);
                var jsonDoc = JsonDocument.Parse(response);
                
                var books = new List<BookSearchResult>();
                
                if (jsonDoc.RootElement.TryGetProperty("items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("volumeInfo", out var volumeInfo))
                        {
                            var book = new BookSearchResult
                            {
                                Title = GetStringProperty(volumeInfo, "title"),
                                Description = GetStringProperty(volumeInfo, "description"),
                                Author = GetAuthorsString(volumeInfo),
                                PublishYear = GetStringProperty(volumeInfo, "publishedDate"),
                                ISBN = GetIsbnString(volumeInfo),
                                CoverImage = GetCoverImageUrl(volumeInfo)
                            };
                            
                            // 只加入有標題的書籍
                            if (!string.IsNullOrEmpty(book.Title))
                            {
                                books.Add(book);
                            }
                        }
                    }
                }

                return books;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Books API 搜尋失敗");
                throw;
            }
        }

        private string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                return property.GetString() ?? string.Empty;
            }
            return string.Empty;
        }

        private string GetAuthorsString(JsonElement volumeInfo)
        {
            if (volumeInfo.TryGetProperty("authors", out var authors) && authors.ValueKind == JsonValueKind.Array)
            {
                var authorList = new List<string>();
                foreach (var author in authors.EnumerateArray())
                {
                    var authorName = author.GetString();
                    if (!string.IsNullOrEmpty(authorName))
                    {
                        authorList.Add(authorName);
                    }
                }
                return string.Join(", ", authorList);
            }
            return "未知作者";
        }

        private string GetIsbnString(JsonElement volumeInfo)
        {
            if (volumeInfo.TryGetProperty("industryIdentifiers", out var identifiers) && identifiers.ValueKind == JsonValueKind.Array)
            {
                foreach (var identifier in identifiers.EnumerateArray())
                {
                    if (identifier.TryGetProperty("type", out var type) && 
                        identifier.TryGetProperty("identifier", out var id))
                    {
                        var typeStr = type.GetString();
                        if (typeStr == "ISBN_13" || typeStr == "ISBN_10")
                        {
                            return id.GetString() ?? string.Empty;
                        }
                    }
                }
            }
            return string.Empty;
        }

        private string GetCoverImageUrl(JsonElement volumeInfo)
        {
            if (volumeInfo.TryGetProperty("imageLinks", out var imageLinks))
            {
                if (imageLinks.TryGetProperty("thumbnail", out var thumbnail))
                {
                    return thumbnail.GetString() ?? string.Empty;
                }
                if (imageLinks.TryGetProperty("smallThumbnail", out var smallThumbnail))
                {
                    return smallThumbnail.GetString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        private List<BookSearchResult> GetMockSearchResults(string query)
        {
            return new List<BookSearchResult>
            {
                new BookSearchResult
                {
                    Title = $"關於「{query}」的書籍範例 1",
                    Description = "這是一本關於您搜尋主題的範例書籍。包含豐富的內容和實用的知識。",
                    Author = "範例作者",
                    PublishYear = "2023",
                    ISBN = "9780000000000",
                    CoverImage = ""
                },
                new BookSearchResult
                {
                    Title = $"「{query}」進階指南",
                    Description = "深入探討相關主題的進階指南，適合想要深入了解的讀者。",
                    Author = "專業作者",
                    PublishYear = "2024",
                    ISBN = "9780000000001",
                    CoverImage = ""
                }
            };
        }

        private string GetSystemPrompt()
        {
            return @"你是一個專業的圖書推薦助手，專門幫助使用者找到相關的書籍。
            
            指導原則：
            1. 優先推薦中文書籍或有中文翻譯的書籍
            2. 提供多樣化的選擇（不同作者、出版社、年份）
            3. 包含經典與新出版的書籍
            4. 確保書籍資訊準確且實用
            5. 針對使用者需求提供最相關的書籍";
        }

        private string[] GetSearchDomains()
        {
            var defaultDomains = new[] { 
                "books.google.com", 
                "amazon.com", 
                "goodreads.com",
                "eslite.com",
                "books.com.tw",
                "kingstone.com.tw",
                "cite.com.tw"
            };
            
            var customDomains = Environment.GetEnvironmentVariable("PERPLEXITY_SEARCH_DOMAINS");
            if (!string.IsNullOrEmpty(customDomains))
            {
                return customDomains.Split(',').Select(d => d.Trim()).ToArray();
            }
            
            return defaultDomains;
        }

        private string GenerateSearchPrompt(string query)
        {
            var maxResults = Environment.GetEnvironmentVariable("PERPLEXITY_MAX_RESULTS") ?? "6";
            
            return $@"請幫我搜尋關於「{query}」的書籍。請提供{maxResults}本相關書籍，每本書請包含以下資訊：

📚 搜尋要求：
- 書名（完整中文書名）
- 作者（包含原作者和譯者，如適用）
- 簡短描述（80-120字，包含主要內容和特色）
- 出版年份（最好是最近幾年的版本）
- ISBN（13位數字格式）
- 推薦理由（為什麼適合此查詢）

🎯 搜尋策略：
1. 優先中文書籍或官方中文翻譯版本
2. 包含不同難度層級（入門、進階）
3. 涵蓋理論與實務應用
4. 考慮經典著作與新近出版

請以下列JSON格式回答：
{{
  ""books"": [
    {{
      ""title"": ""書名"",
      ""author"": ""作者（含譯者）"",
      ""description"": ""詳細描述"",
      ""publishYear"": ""出版年份"",
      ""isbn"": ""ISBN號碼"",
      ""recommendation"": ""推薦理由""
    }}
  ]
}}

🌟 請確保推薦的書籍真實存在且資訊準確。";
        }
    }
}