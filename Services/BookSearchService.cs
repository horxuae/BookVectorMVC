using BookVectorMVC.Services.Interfaces;
using BookVectorMVC.Controllers;
using System.Text.Json;

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
                // 使用Google Books API進行搜尋
                var encodedQuery = Uri.EscapeDataString(query);
                var apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={encodedQuery}&maxResults=10&langRestrict=zh";
                
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
                _logger.LogError(ex, "搜尋線上書籍時發生錯誤");
                
                // 如果Google Books API失敗，返回模擬的搜尋結果
                return GetMockSearchResults(query);
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
    }
}