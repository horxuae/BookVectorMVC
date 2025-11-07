using BookVectorMVC.Models;
using BookVectorMVC.Models.ViewModels;
using BookVectorMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookVectorMVC.Controllers;

/// <summary>
/// 增強版書籍控制器 - 提供進階功能和統計資訊
/// </summary>
public class EnhancedBookController : Controller
{
    private readonly IBookService _bookService;
    private readonly IEnhancedBookService _enhancedBookService;
    private readonly ILogger<EnhancedBookController> _logger;

    public EnhancedBookController(
        IBookService bookService, 
        IEnhancedBookService enhancedBookService,
        ILogger<EnhancedBookController> logger)
    {
        _bookService = bookService;
        _enhancedBookService = enhancedBookService;
        _logger = logger;
    }

    /// <summary>
    /// 增強版首頁 - 包含統計資訊和進階功能
    /// </summary>
    public async Task<IActionResult> Enhanced(CancellationToken cancellationToken = default)
    {
        try
        {
            var books = await LoadBooksAsync(cancellationToken);
            ViewBag.Statistics = await _enhancedBookService.GetSystemStatisticsAsync(cancellationToken);
            ViewBag.VectorAnalysis = await _enhancedBookService.GetVectorQualityAnalysisAsync(cancellationToken);
            return View("~/Views/Book/Enhanced.cshtml", books);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading enhanced view");
            ViewBag.Error = "載入頁面時發生錯誤";
            return View("~/Views/Book/Enhanced.cshtml", new List<Book>());
        }
    }

    /// <summary>
    /// 新增書籍 (增強版)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(BookViewModel model, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "請確認輸入資料的正確性";
            return RedirectToAction(nameof(Enhanced));
        }

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            TempData["Error"] = "書名不能為空";
            return RedirectToAction(nameof(Enhanced));
        }

        try
        {
            var book = await _bookService.AddBookAsync(model.Title, model.Description, model.Position, cancellationToken);
            TempData["Success"] = $"已成功新增《{book.Title}》";
            _logger.LogInformation("Successfully added book: {Title} (ID: {BookId})", book.Title, book.BookId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book: {Title}", model.Title);
            TempData["Error"] = "新增時發生問題: " + ex.Message;
        }

        return RedirectToAction(nameof(Enhanced));
    }

    /// <summary>
    /// 搜尋書籍 (增強版)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Search(SearchViewModel model, CancellationToken cancellationToken = default)
    {
        var results = new List<SearchResult>();

        try
        {
            if (!string.IsNullOrWhiteSpace(model.Position))
            {
                results = await _bookService.SearchByPositionAsync(model.Position, cancellationToken);
                ViewBag.SearchMethod = "依位置搜尋";
                ViewBag.SearchTerm = model.Position;
            }
            else if (!string.IsNullOrWhiteSpace(model.Title))
            {
                results = await _bookService.SearchByTitleAsync(model.Title, cancellationToken);
                ViewBag.SearchMethod = "依書名搜尋";
                ViewBag.SearchTerm = model.Title;
            }
            else if (!string.IsNullOrWhiteSpace(model.Query))
            {
                results = await _bookService.SearchByVectorAsync(model.Query, 10, cancellationToken);
                ViewBag.SearchMethod = "依向量搜尋";
                ViewBag.SearchQuery = model.Query;
                ViewBag.SearchTerm = model.Query;
            }
            else
            {
                ViewBag.SearchMethod = "請輸入搜尋條件";
                ViewBag.SearchTerm = "";
            }

            ViewBag.ResultCount = results.Count;
            
            // 如果是向量搜尋，提供額外的搜尋洞察
            if (!string.IsNullOrWhiteSpace(model.Query) && results.Any())
            {
                ViewBag.HighestScore = results.Max(r => r.Score);
                ViewBag.LowestScore = results.Min(r => r.Score);
                ViewBag.AvgScore = results.Average(r => r.Score);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during enhanced search");
            ViewBag.Error = "搜尋發生異常: " + ex.Message;
        }

        return PartialView("~/Views/Book/_EnhancedSearchResult.cshtml", results);
    }

    /// <summary>
    /// 獲取系統統計 API
    /// </summary>
    //[HttpGet]
    //public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var stats = await _enhancedBookService.GetSystemStatisticsAsync(cancellationToken);
    //        return Json(stats);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting statistics");
    //        return Json(new { error = "獲取統計資料時發生錯誤" });
    //    }
    //}

    /// <summary>
    /// 獲取向量品質分析 API
    /// </summary>
    //[HttpGet]
    //public async Task<IActionResult> GetVectorAnalysis(CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var analysis = await _enhancedBookService.GetVectorQualityAnalysisAsync(cancellationToken);
    //        return Json(analysis);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting vector analysis");
    //        return Json(new { error = "獲取向量分析時發生錯誤" });
    //    }
    //}

    /// <summary>
    /// 獲取搜尋效能統計 API
    /// </summary>
    //[HttpGet]
    //public async Task<IActionResult> GetPerformanceStats(CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        var stats = await _enhancedBookService.GetSearchPerformanceStatsAsync(cancellationToken);
    //        return Json(stats);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error getting performance stats");
    //        return Json(new { error = "獲取效能統計時發生錯誤" });
    //    }
    //}

    /// <summary>
    /// 批量更新向量
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAllVectors(CancellationToken cancellationToken = default)
    {
        try
        {
            var updatedCount = await _bookService.UpdateAllVectorsAsync(cancellationToken);
            TempData["Success"] = $"已更新 {updatedCount} 本書籍的向量表示";
            _logger.LogInformation("Bulk updated vectors for {Count} books", updatedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating all vectors");
            TempData["Error"] = "批量更新向量時發生錯誤: " + ex.Message;
        }

        return RedirectToAction(nameof(Enhanced));
    }

    #region 私有方法

    /// <summary>
    /// 載入所有書籍，包含錯誤處理
    /// </summary>
    private async Task<List<Book>> LoadBooksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _bookService.GetAllBooksAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading books");
            ViewBag.Error = "載入書籍發生異常: " + ex.Message;
            return new List<Book>();
        }
    }

    #endregion
}