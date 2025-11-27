using System.ComponentModel.DataAnnotations;

namespace BookVectorMVC.Models;

/// <summary>
/// 使用者
/// </summary>
public class User
{
    /// <summary>
    /// 識別碼
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    [Required(ErrorMessage = "使用者名稱不能為空")]
    [StringLength(50, ErrorMessage = "使用者名稱長度不能超過 50 個字元")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    [Required(ErrorMessage = "電子郵件不能為空")]
    [EmailAddress(ErrorMessage = "請輸入有效的電子郵件地址")]
    [StringLength(100, ErrorMessage = "電子郵件長度不能超過 100 個字元")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    [Required(ErrorMessage = "密碼不能為空")]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 名稱
    /// </summary>
    [StringLength(100, ErrorMessage = "顯示名稱長度不能超過 100 個字元")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 角色 (Admin, Member)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string Role { get; set; } = "Member";

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 最後登入日期
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; } = true;
}