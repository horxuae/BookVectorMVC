namespace BookVectorMVC.Services;

/// <summary>
/// API 服務介面 - 處理外部 AI 向量化服務
/// </summary>
public interface IApiService
{
    /// <summary>
    /// 向量維度
    /// </summary>
    int VectorDimension { get; }

    /// <summary>
    /// 取得文字的語意向量
    /// </summary>
    /// <param name="text">輸入文字</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>浮點向量陣列</returns>
    Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default);
}