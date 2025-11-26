using System.ComponentModel.DataAnnotations;

namespace BookVectorMVC.Models.ViewModels;

/// <summary>
/// 使用者註冊
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "使用者名稱不能為空")]
    [StringLength(50, ErrorMessage = "使用者名稱長度不能超過 50 個字元")]
    [Display(Name = "使用者名稱")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "電子郵件不能為空")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [Display(Name = "電子郵件")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼不能為空")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "密碼長度必須在 6 到 100 個字元之間")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "確認密碼")]
    [Compare("Password", ErrorMessage = "密碼和確認密碼不匹配")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "顯示名稱長度不能超過 100 個字元")]
    [Display(Name = "顯示名稱")]
    public string? DisplayName { get; set; }
}

/// <summary>
/// 使用者登入
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "使用者名稱不能為空")]
    [Display(Name = "使用者名稱")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "密碼不能為空")]
    [DataType(DataType.Password)]
    [Display(Name = "密碼")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "記住我")]
    public bool RememberMe { get; set; }
}

/// <summary>
/// 使用者編輯
/// </summary>
public class EditBookViewModel
{
    public int BookId { get; set; }

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