using Microsoft.AspNetCore.Mvc;
using BookVectorMVC.Services.Interfaces;
using BookVectorMVC.Models;
using BookVectorMVC.Data;
using System.Text.Json;

namespace BookVectorMVC.Controllers
{
    public class ChatBotController : Controller
    {
        private readonly IBookSearchService _bookSearchService;
        private readonly BookDbContext _context;

        public ChatBotController(IBookSearchService bookSearchService, BookDbContext context)
        {
            _bookSearchService = bookSearchService;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SearchBooks([FromBody] ChatSearchRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return Json(new { success = false, message = "請輸入搜尋關鍵字" });
                }

                var searchResults = await _bookSearchService.SearchOnlineBooks(request.Query);
                
                return Json(new { 
                    success = true, 
                    books = searchResults.Select(book => new {
                        title = book.Title,
                        description = book.Description,
                        author = book.Author,
                        isbn = book.ISBN,
                        publishYear = book.PublishYear,
                        coverImage = book.CoverImage
                    })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "搜尋時發生錯誤：" + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddSelectedBooks([FromBody] AddBooksRequest request)
        {
            try
            {
                if (User.Identity?.IsAuthenticated != true || !User.IsInRole("Admin"))
                {
                    return Json(new { success = false, message = "只有管理員才能新增書籍" });
                }

                if (request?.SelectedBooks == null || !request.SelectedBooks.Any())
                {
                    return Json(new { success = false, message = "沒有選擇任何書籍" });
                }

                // 直接使用資料庫上下文新增書籍
                var addedCount = 0;
                foreach (var bookData in request.SelectedBooks)
                {
                    try
                    {
                        // 檢查書籍是否已存在（根據標題和作者）
                        var existingBook = _context.Books.FirstOrDefault(b => 
                            b.Title.ToLower() == bookData.Title.ToLower());

                        if (existingBook == null)
                        {
                            var newBook = new Book
                            {
                                Title = bookData.Title,
                                Description = $"{bookData.Description}\n\n作者：{bookData.Author}\nISBN：{bookData.ISBN}\n出版年份：{bookData.PublishYear}",
                                Position = "", // 預設為空，讓使用者稍後編輯
                                Vector = "[]"
                            };

                            _context.Books.Add(newBook);
                            addedCount++;
                        }
                    }
                    catch (Exception bookEx)
                    {
                        // 記錄單一書籍新增失敗，但繼續處理其他書籍
                        Console.WriteLine($"新增書籍失敗：{bookData.Title} - {bookEx.Message}");
                    }
                }

                // 儲存所有變更
                if (addedCount > 0)
                {
                    await _context.SaveChangesAsync();
                }

                if (addedCount > 0)
                {
                    return Json(new { 
                        success = true, 
                        message = $"成功新增 {addedCount} 本書籍到資料庫！請重新整理頁面查看。" 
                    });
                }
                else
                {
                    return Json(new { success = false, message = "沒有成功新增任何書籍，請稍後再試" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "新增書籍時發生錯誤：" + ex.Message });
            }
        }
    }

    public class ChatSearchRequest
    {
        public string Query { get; set; } = string.Empty;
    }

    public class AddBooksRequest
    {
        public List<BookSearchResult> SelectedBooks { get; set; } = new();
    }

    public class BookSearchResult
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string ISBN { get; set; } = string.Empty;
        public string PublishYear { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
    }
}