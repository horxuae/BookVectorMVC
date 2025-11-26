using BookVectorMVC.Models;
using BookVectorMVC.Models.ViewModels;

namespace BookVectorMVC.Services.Interfaces;

/// <summary>
/// 使用者
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 使用者註冊
    /// </summary>
    /// <param name="model">註冊視圖模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>註冊結果</returns>
    Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 使用者登入
    /// </summary>
    /// <param name="model">登入視圖模型</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登入結果</returns>
    Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據使用者名稱獲取使用者
    /// </summary>
    /// <param name="username">使用者名稱</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>使用者實體</returns>
    Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根據 ID 獲取使用者
    /// </summary>
    /// <param name="id">使用者 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>使用者實體</returns>
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 更新最後登入時間
    /// </summary>
    /// <param name="userId">使用者 ID</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// 驗證密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <param name="hashedPassword">加密密碼</param>
    /// <returns>是否匹配</returns>
    bool VerifyPassword(string password, string hashedPassword);

    /// <summary>
    /// 加密密碼
    /// </summary>
    /// <param name="password">明文密碼</param>
    /// <returns>加密後的密碼</returns>
    string HashPassword(string password);
}