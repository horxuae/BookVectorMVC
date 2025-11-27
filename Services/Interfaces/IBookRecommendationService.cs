using BookVectorMVC.Models;

namespace BookVectorMVC.Services.Interfaces
{
    public interface IBookRecommendationService
    {
        /// <summary>
        /// 基於使用者偏好推薦書籍
        /// </summary>
        Task<List<Book>> GetPersonalizedRecommendations(string userId, int count = 5);
        
        /// <summary>
        /// 基於已讀書籍推薦相似書籍
        /// </summary>
        Task<List<Book>> GetSimilarBooks(int bookId, int count = 5);
        
        /// <summary>
        /// 智能分析書籍描述並生成標籤
        /// </summary>
        Task<List<string>> GenerateBookTags(string description);
        
        /// <summary>
        /// 自動分類書籍類型
        /// </summary>
        Task<string> ClassifyBookCategory(string title, string description);
        
        /// <summary>
        /// 生成書籍摘要
        /// </summary>
        Task<string> GenerateBookSummary(string title, string description);
        
        /// <summary>
        /// 智能問答：根據圖書館內容回答問題
        /// </summary>
        Task<string> AnswerLibraryQuestion(string question);
    }
}