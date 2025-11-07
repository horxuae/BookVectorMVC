using BookVectorMVC.Models;

namespace BookVectorMVC.Services;

/// <summary>
/// 書籍服務介面
/// </summary>
public interface IBookService
{
    /// <summary>
    /// 獲取所有書籍
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>書籍清單</returns>
    Task<List<Book>> GetAllBooksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 獲取書籍
    /// </summary>
    /// <param name="id">書籍 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>書籍實體，若不存在則返回 null</returns>
    Task<Book?> GetBookByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 新增書籍
    /// </summary>
    /// <param name="title">書名</param>
    /// <param name="description">描述</param>
    /// <param name="position">位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新增的書籍實體</returns>
    Task<Book> AddBookAsync(string title, string? description, string? position, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新書籍
    /// </summary>
    /// <param name="book">書籍實體</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新後的書籍實體</returns>
    Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default);

    /// <summary>
    /// 刪除書籍
    /// </summary>
    /// <param name="id">書籍 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功刪除</returns>
    Task<bool> DeleteBookAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 基於向量相似度的智能搜尋
    /// </summary>
    /// <param name="query">搜尋查詢文本</param>
    /// <param name="maxResults">最大結果數量</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>按相似度排序的搜尋結果</returns>
    Task<List<SearchResult>> SearchByVectorAsync(string query, int maxResults = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據位置搜尋書籍
    /// </summary>
    /// <param name="position">位置</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配位置的書籍搜尋結果</returns>
    Task<List<SearchResult>> SearchByPositionAsync(string position, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據書名搜尋書籍
    /// </summary>
    /// <param name="title">書名關鍵字</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>標題匹配的搜尋結果</returns>
    Task<List<SearchResult>> SearchByTitleAsync(string title, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新所有書籍的向量
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>更新的書籍數量</returns>
    Task<int> UpdateAllVectorsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 將向量陣列序列化為 JSON 字符串
    /// </summary>
    /// <param name="vector">向量陣列</param>
    /// <returns>JSON 格式的字符串</returns>
    string SerializeVector(float[] vector);

    /// <summary>
    /// 將 JSON 字符串反序列化為向量陣列
    /// </summary>
    /// <param name="json">JSON 格式的字符串</param>
    /// <returns>向量陣列</returns>
    float[] DeserializeVector(string json);
}