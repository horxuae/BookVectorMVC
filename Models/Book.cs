using System.ComponentModel.DataAnnotations;

namespace BookVectorMVC.Models;

/// <summary>
/// 書籍實體模型
/// </summary>
public class Book
{
    /// <summary>
    /// 書籍唯一識別碼
    /// </summary>
    public int BookId { get; set; }

    /// <summary>
    /// 書名
    /// </summary>
    [Required(ErrorMessage = "書名不能為空")]
    [StringLength(200, ErrorMessage = "書名長度不能超過 200 個字元")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 書籍描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 書籍實體位置 (如: A-12, B-05)
    /// </summary>
    [StringLength(100, ErrorMessage = "位置長度不能超過 100 個字元")]
    public string? Position { get; set; }

    /// <summary>
    /// 向量表示 (JSON 格式儲存)
    /// </summary>
    public string Vector { get; set; } = "[]";
}
