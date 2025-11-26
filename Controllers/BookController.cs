using BookVectorMVC.Models;
using BookVectorMVC.Models.ViewModels;
using BookVectorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookVectorMVC.Controllers;

/// <summary>
/// 書籍控制器 - 處理基本的書籍管理功能
/// </summary>
public class BookController : Controller
{
    private readonly IBookService _bookService;
    private readonly ILogger<BookController> _logger;

    public BookController(IBookService bookService, ILogger<BookController> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /// <summary>
    /// 首頁 - 顯示所有書籍和搜尋功能
    /// </summary>
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        try
        {
            var books = await _bookService.GetAllBooksAsync(cancellationToken);
            return View(books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading books in Index");
            TempData["Error"] = "載入書籍時發生錯誤";
            return View(new List<Book>());
        }
    }

    /// <summary>
    /// 新增書籍
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BookViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "請確認輸入資料的正確性";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _bookService.AddBookAsync(model.Title, model.Description, model.Position, cancellationToken);
            TempData["Success"] = $"成功新增書籍：{model.Title}";
            _logger.LogInformation("Successfully added book: {Title}", model.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book: {Title}", model.Title);
            TempData["Error"] = "新增書籍時發生錯誤";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 搜尋書籍
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            List<SearchResult> results = new();

            if (!string.IsNullOrWhiteSpace(model.Position))
            {
                results = await _bookService.SearchByPositionAsync(model.Position, cancellationToken);
                ViewBag.SearchMethod = "位置搜尋";
                ViewBag.SearchTerm = model.Position;
            }
            else if (!string.IsNullOrWhiteSpace(model.Title))
            {
                results = await _bookService.SearchByTitleAsync(model.Title, cancellationToken);
                ViewBag.SearchMethod = "書名搜尋";
                ViewBag.SearchTerm = model.Title;
            }
            else if (!string.IsNullOrWhiteSpace(model.Query))
            {
                results = await _bookService.SearchByVectorAsync(model.Query, 10, cancellationToken);
                ViewBag.SearchMethod = "語義搜尋";
                ViewBag.SearchTerm = model.Query;
            }
            else
            {
                ViewBag.SearchMethod = "請輸入搜尋條件";
                ViewBag.SearchTerm = "";
            }

            ViewBag.ResultCount = results.Count;
            return PartialView("_SearchResult", results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during search");
            ViewBag.Error = "搜尋時發生錯誤";
            return PartialView("_SearchResult", new List<SearchResult>());
        }
    }

    /// <summary>
    /// 獲取書籍詳細資訊
    /// </summary>
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var book = await _bookService.GetBookByIdAsync(id, cancellationToken);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading book details for ID: {BookId}", id);
            return NotFound();
        }
    }

    /// <summary>
    /// 編輯書籍頁面
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var book = await _bookService.GetBookByIdAsync(id, cancellationToken);
            if (book == null)
            {
                TempData["Error"] = "找不到指定的書籍";
                return RedirectToAction(nameof(Index));
            }

            var model = new BookVectorMVC.Models.ViewModels.EditBookViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Description = book.Description,
                Position = book.Position
            };

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading book for edit: {BookId}", id);
            TempData["Error"] = "載入書籍資料時發生錯誤";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 編輯書籍處理
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookVectorMVC.Models.ViewModels.EditBookViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var book = await _bookService.GetBookByIdAsync(model.BookId, cancellationToken);
            if (book == null)
            {
                TempData["Error"] = "找不到指定的書籍";
                return RedirectToAction(nameof(Index));
            }

            book.Title = model.Title;
            book.Description = model.Description;
            book.Position = model.Position;

            await _bookService.UpdateBookAsync(book, cancellationToken);
            TempData["Success"] = $"成功更新書籍：{book.Title}";
            _logger.LogInformation("Successfully updated book: {Title} (ID: {BookId})", book.Title, book.BookId);
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book: {BookId}", model.BookId);
            TempData["Error"] = "更新書籍時發生錯誤";
            return View(model);
        }
    }

    /// <summary>
    /// 刪除書籍
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _bookService.DeleteBookAsync(id, cancellationToken);
            if (success)
            {
                TempData["Success"] = "書籍已成功刪除";
                _logger.LogInformation("Successfully deleted book with ID: {BookId}", id);
            }
            else
            {
                TempData["Error"] = "找不到指定的書籍";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book with ID: {BookId}", id);
            TempData["Error"] = "刪除書籍時發生錯誤";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// 更新所有書籍向量
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateVectors(CancellationToken cancellationToken = default)
    {
        try
        {
            var updatedCount = await _bookService.UpdateAllVectorsAsync(cancellationToken);
            TempData["Success"] = $"已更新 {updatedCount} 本書籍的向量";
            _logger.LogInformation("Updated vectors for {Count} books", updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating all vectors");
            TempData["Error"] = "更新向量時發生錯誤";
        }

        return RedirectToAction(nameof(Index));
    }
}