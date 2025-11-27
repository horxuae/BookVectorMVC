using Microsoft.AspNetCore.Mvc;
using BookVectorMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace BookVectorMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AIAssistantController : Controller
    {
        private readonly IBookRecommendationService _recommendationService;
        private readonly ILogger<AIAssistantController> _logger;

        public AIAssistantController(IBookRecommendationService recommendationService, ILogger<AIAssistantController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTags([FromBody] GenerateTagsRequest request)
        {
            try
            {
                var tags = await _recommendationService.GenerateBookTags(request.Description);
                return Json(new { success = true, tags = tags });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成標籤時發生錯誤");
                return Json(new { success = false, message = "生成標籤失敗" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClassifyBook([FromBody] ClassifyBookRequest request)
        {
            try
            {
                var category = await _recommendationService.ClassifyBookCategory(request.Title, request.Description);
                return Json(new { success = true, category = category });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "分類書籍時發生錯誤");
                return Json(new { success = false, message = "書籍分類失敗" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GenerateSummary([FromBody] GenerateSummaryRequest request)
        {
            try
            {
                var summary = await _recommendationService.GenerateBookSummary(request.Title, request.Description);
                return Json(new { success = true, summary = summary });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成摘要時發生錯誤");
                return Json(new { success = false, message = "生成摘要失敗" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetSimilarBooks([FromBody] SimilarBooksRequest request)
        {
            try
            {
                var books = await _recommendationService.GetSimilarBooks(request.BookId, request.Count);
                return Json(new { 
                    success = true, 
                    books = books.Select(b => new {
                        id = b.BookId,
                        title = b.Title,
                        description = b.Description,
                        position = b.Position
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取相似書籍時發生錯誤");
                return Json(new { success = false, message = "獲取相似書籍失敗" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AnswerQuestion([FromBody] QuestionRequest request)
        {
            try
            {
                var answer = await _recommendationService.AnswerLibraryQuestion(request.Question);
                return Json(new { success = true, answer = answer });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回答問題時發生錯誤");
                return Json(new { success = false, message = "回答問題失敗" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonalizedRecommendations(int count = 5)
        {
            try
            {
                var userId = User.Identity?.Name ?? "anonymous";
                var books = await _recommendationService.GetPersonalizedRecommendations(userId, count);
                
                return Json(new { 
                    success = true, 
                    books = books.Select(b => new {
                        id = b.BookId,
                        title = b.Title,
                        description = b.Description,
                        position = b.Position
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "獲取個人化推薦時發生錯誤");
                return Json(new { success = false, message = "獲取推薦失敗" });
            }
        }
    }

    // Request DTOs
    public class GenerateTagsRequest
    {
        public string Description { get; set; } = string.Empty;
    }

    public class ClassifyBookRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class GenerateSummaryRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class SimilarBooksRequest
    {
        public int BookId { get; set; }
        public int Count { get; set; } = 5;
    }

    public class QuestionRequest
    {
        public string Question { get; set; } = string.Empty;
    }
}