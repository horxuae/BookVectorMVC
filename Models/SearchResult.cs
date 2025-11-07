namespace BookVectorMVC.Models;

/// <summary>
/// 搜尋結果模型
/// </summary>
public class SearchResult
{
    /// <summary>
    /// 搜尋到的書籍
    /// </summary>
    public Book Book { get; set; } = new();

    /// <summary>
    /// 相似度分數 (0-1，越高越相似)
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// 搜尋結果排名
    /// </summary>
    public int Rank { get; set; }
}
