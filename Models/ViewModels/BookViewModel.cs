using System.ComponentModel.DataAnnotations;

namespace BookVectorMVC.Models.ViewModels;

/// <summary>
/// 書籍新增/編輯的視圖模型
/// </summary>
public class BookViewModel
{
    [Required(ErrorMessage = "書名不能為空")]
    [StringLength(200, ErrorMessage = "書名長度不能超過 200 個字元")]
    [Display(Name = "書名")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "描述")]
    public string? Description { get; set; }

    [StringLength(100, ErrorMessage = "位置長度不能超過 100 個字元")]
    [Display(Name = "位置")]
    public string? Position { get; set; }
}

/// <summary>
/// 搜尋查詢的視圖模型
/// </summary>
public class SearchViewModel
{
    [Display(Name = "關鍵字搜尋")]
    public string? Query { get; set; }

    [Display(Name = "位置搜尋")]
    public string? Position { get; set; }

    [Display(Name = "書名搜尋")]
    public string? Title { get; set; }
}