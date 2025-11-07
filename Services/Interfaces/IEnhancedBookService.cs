namespace BookVectorMVC.Services;

/// <summary>
/// 增強書籍服務介面
/// </summary>
public interface IEnhancedBookService
{
    /// <summary>
    /// 獲取系統統計資訊
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>系統統計資料</returns>
    Task<Dictionary<string, object>> GetSystemStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 獲取向量品質分析
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>向量品質分析結果</returns>
    Task<Dictionary<string, object>> GetVectorQualityAnalysisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 獲取搜尋效能統計
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>搜尋效能統計</returns>
    //Task<Dictionary<string, object>> GetSearchPerformanceStatsAsync(CancellationToken cancellationToken = default);
}