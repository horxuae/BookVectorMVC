namespace BookVectorMVC.Services;

/// <summary>
/// 增強書籍服務實作
/// </summary>
public class EnhancedBookService : IEnhancedBookService
{
    private readonly IBookService _bookService;
    private readonly ILogger<EnhancedBookService> _logger;

    public EnhancedBookService(IBookService bookService, ILogger<EnhancedBookService> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    /// <summary>
    /// 獲取系統統計資訊，包含向量化相關的分析數據
    /// </summary>
    public async Task<Dictionary<string, object>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var books = await _bookService.GetAllBooksAsync(cancellationToken);
        var stats = new Dictionary<string, object>();

        // 基本統計
        stats["TotalBooks"] = books.Count;
        stats["AvgTitleLength"] = books.Count > 0 ? books.Average(b => b.Title?.Length ?? 0) : 0;
        stats["AvgDescriptionLength"] = books.Count > 0 ? books.Average(b => b.Description?.Length ?? 0) : 0;
        stats["UniquePositions"] = books.Select(b => b.Position).Where(p => !string.IsNullOrEmpty(p)).Distinct().Count();
        stats["BooksWithVectors"] = books.Count(b => !string.IsNullOrEmpty(b.Vector) && b.Vector != "[]");

        // 向量維度統計
        var vectorLengths = books
            .Where(b => !string.IsNullOrEmpty(b.Vector) && b.Vector != "[]")
            .Select(b => _bookService.DeserializeVector(b.Vector)?.Length ?? 0)
            .Where(len => len > 0)
            .ToList();

        if (vectorLengths.Any())
        {
            stats["AvgVectorDimension"] = vectorLengths.Average();
            stats["MaxVectorDimension"] = vectorLengths.Max();
            stats["MinVectorDimension"] = vectorLengths.Min();
            stats["VectorConsistency"] = vectorLengths.Distinct().Count() == 1 ? "一致" : "不一致";
        }
        else
        {
            stats["AvgVectorDimension"] = 0;
            stats["MaxVectorDimension"] = 0;
            stats["MinVectorDimension"] = 0;
            stats["VectorConsistency"] = "無向量資料";
        }

        // 資料品質統計
        stats["BooksWithoutDescription"] = books.Count(b => string.IsNullOrEmpty(b.Description));
        stats["BooksWithoutPosition"] = books.Count(b => string.IsNullOrEmpty(b.Position));
        stats["DataQualityScore"] = CalculateDataQualityScore(books);

        _logger.LogInformation("Generated system statistics for {BookCount} books", books.Count);
        return stats;
    }

    /// <summary>
    /// 獲取向量品質分析
    /// </summary>
    public async Task<Dictionary<string, object>> GetVectorQualityAnalysisAsync(CancellationToken cancellationToken = default)
    {
        var books = await _bookService.GetAllBooksAsync(cancellationToken);
        var analysis = new Dictionary<string, object>();

        var vectors = books
            .Where(b => !string.IsNullOrEmpty(b.Vector) && b.Vector != "[]")
            .Select(b => _bookService.DeserializeVector(b.Vector))
            .Where(v => v.Length > 0)
            .ToList();

        if (!vectors.Any())
        {
            analysis["Status"] = "無可分析的向量資料";
            return analysis;
        }

        // 向量統計
        analysis["TotalVectors"] = vectors.Count;
        analysis["VectorDimensions"] = vectors.First().Length;

        // 計算向量的統計特徵
        var allValues = vectors.SelectMany(v => v).ToList();
        analysis["VectorValueStats"] = new Dictionary<string, object>
        {
            ["Mean"] = allValues.Average(),
            ["Min"] = allValues.Min(),
            ["Max"] = allValues.Max(),
            ["StandardDeviation"] = CalculateStandardDeviation(allValues)
        };

        // 向量相似度分析
        var similarities = new List<double>();
        for (int i = 0; i < Math.Min(vectors.Count, 10); i++)
        {
            for (int j = i + 1; j < Math.Min(vectors.Count, 10); j++)
            {
                var similarity = CalculateCosineSimilarity(vectors[i], vectors[j]);
                similarities.Add(similarity);
            }
        }

        if (similarities.Any())
        {
            analysis["SimilarityStats"] = new Dictionary<string, object>
            {
                ["AvgSimilarity"] = similarities.Average(),
                ["MinSimilarity"] = similarities.Min(),
                ["MaxSimilarity"] = similarities.Max()
            };
        }

        return analysis;
    }

    /// <summary>
    /// 獲取搜尋效能統計
    /// </summary>
    //public async Task<Dictionary<string, object>> GetSearchPerformanceStatsAsync(CancellationToken cancellationToken = default)
    //{
    //    var stats = new Dictionary<string, object>();
    //    var books = await _bookService.GetAllBooksAsync(cancellationToken);
    //
    //    // 模擬搜尋效能測試
    //    var testQueries = new[] { "程式設計", "資料結構", "演算法", "機器學習", "人工智慧" };
    //    var searchTimes = new List<double>();
    //
    //    foreach (var query in testQueries.Take(3)) // 限制測試數量以避免過度使用 API
    //    {
    //        var startTime = DateTime.UtcNow;
    //        try
    //        {
    //            await _bookService.SearchByVectorAsync(query, 5, cancellationToken);
    //            var endTime = DateTime.UtcNow;
    //            searchTimes.Add((endTime - startTime).TotalMilliseconds);
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogWarning(ex, "Search performance test failed for query: {Query}", query);
    //        }
    //    }
    //
    //    if (searchTimes.Any())
    //    {
    //        stats["AvgSearchTime"] = $"{searchTimes.Average():F2} ms";
    //        stats["MinSearchTime"] = $"{searchTimes.Min():F2} ms";
    //        stats["MaxSearchTime"] = $"{searchTimes.Max():F2} ms";
    //    }
    //
    //    stats["TotalBooks"] = books.Count;
    //    stats["EstimatedSearchComplexity"] = $"O({books.Count})";
    //
    //    return stats;
    //}

    #region 私有輔助方法

    private static double CalculateDataQualityScore(List<Models.Book> books)
    {
        if (!books.Any()) return 0;

        var score = 0.0;
        var totalChecks = 0;

        foreach (var book in books)
        {
            // 書名完整性
            if (!string.IsNullOrEmpty(book.Title))
            {
                score += 0.4;
                if (book.Title.Length > 5) score += 0.1;
            }
            totalChecks += (int)0.5;

            // 描述完整性
            if (!string.IsNullOrEmpty(book.Description))
            {
                score += 0.2;
                if (book.Description.Length > 20) score += 0.1;
            }
            totalChecks += (int)0.3;

            // 向量完整性
            if (!string.IsNullOrEmpty(book.Vector) && book.Vector != "[]")
            {
                score += 0.2;
            }
            totalChecks += (int)0.2;
        }

        return totalChecks > 0 ? (score / totalChecks) * 100 : 0;
    }

    private static double CalculateStandardDeviation(List<float> values)
    {
        if (!values.Any()) return 0;

        var mean = values.Average();
        var variance = values.Average(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(variance);
    }

    private static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length == 0 || vectorB.Length == 0) return 0.0;

        var minLength = Math.Min(vectorA.Length, vectorB.Length);
        double dotProduct = 0, normA = 0, normB = 0;

        for (int i = 0; i < minLength; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator > 0 ? dotProduct / denominator : 0.0;
    }

    #endregion
}