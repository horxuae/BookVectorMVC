using System.Collections.Generic;
using System.Linq;

namespace BookVectorMVC.Services
{
    public class EnhancedBookService
    {
        private BookService bookService = new BookService();

        /// <summary>
        /// 獲取系統統計資訊，包含向量化相關的分析數據
        /// </summary>
        public Dictionary<string, object> GetSystemStatistics()
        {
            var books = bookService.GetAllBooks();
            var stats = new Dictionary<string, object>();

            // 基本統計
            stats["TotalBooks"] = books.Count;
            stats["AvgTitleLength"] = books.Count > 0 ? books.Average(b => b.Title?.Length ?? 0) : 0;
            stats["AvgDescriptionLength"] = books.Count > 0 ? books.Average(b => b.Description?.Length ?? 0) : 0;
            stats["UniquePositions"] = books.Select(b => b.Position).Distinct().Count();
            stats["BooksWithVectors"] = books.Count(b => !string.IsNullOrEmpty(b.Vector) && b.Vector != "[]");

            // 若資料有異常可檢查一致性
            var vectorLengths = books.Where(b => !string.IsNullOrEmpty(b.Vector) && b.Vector != "[]")
                                     .Select(b => bookService.UnpackVector(b.Vector)?.Length ?? 0)
                                     .Where(len => len > 0)
                                     .ToList();
            if (vectorLengths.Any())
            {
                // 平均維度：幫助了解系統整體的向量複雜度
                stats["AvgVectorDimension"] = vectorLengths.Average();
                // 最大/最小維度：檢查向量維度的一致性
                stats["MaxVectorDimension"] = vectorLengths.Max();
                stats["MinVectorDimension"] = vectorLengths.Min();
            }

            return stats;
        }
    }
}