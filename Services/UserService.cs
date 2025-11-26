using BookVectorMVC.Data;
using BookVectorMVC.Models;
using BookVectorMVC.Models.ViewModels;
using BookVectorMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BookVectorMVC.Services;

/// <summary>
/// 使用者服務實作
/// </summary>
public class UserService : IUserService
{
    private readonly BookDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(BookDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 使用者註冊
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            // 檢查使用者名稱是否已存在
            var existingUser = await GetUserByUsernameAsync(model.Username, cancellationToken);
            if (existingUser != null)
            {
                return (false, "使用者名稱已存在", null);
            }

            // 檢查電子郵件是否已存在
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email, cancellationToken);
            if (existingEmail != null)
            {
                return (false, "電子郵件已被註冊", null);
            }

            // 建立新使用者
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                DisplayName = model.DisplayName ?? model.Username,
                Role = "Member",
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("New user registered: {Username}", model.Username);
            return (true, "註冊成功", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Username}", model.Username);
            return (false, "註冊時發生錯誤", null);
        }
    }

    /// <summary>
    /// 使用者登入
    /// </summary>
    public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByUsernameAsync(model.Username, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return (false, "使用者名稱或密碼錯誤", null);
            }

            if (!VerifyPassword(model.Password, user.PasswordHash))
            {
                return (false, "使用者名稱或密碼錯誤", null);
            }

            // 更新最後登入時間
            await UpdateLastLoginAsync(user.UserId, cancellationToken);

            _logger.LogInformation("User logged in: {Username}", model.Username);
            return (true, "登入成功", user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login: {Username}", model.Username);
            return (false, "登入時發生錯誤", null);
        }
    }

    /// <summary>
    /// 根據使用者名稱獲取使用者
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <summary>
    /// 根據 ID 獲取使用者
    /// </summary>
    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
    }

    /// <summary>
    /// 更新最後登入時間
    /// </summary>
    public async Task<bool> UpdateLastLoginAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await GetUserByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.LastLoginDate = DateTime.Now;
                await _context.SaveChangesAsync(cancellationToken);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    /// <summary>
    /// 驗證密碼
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return HashPassword(password) == hashedPassword;
    }

    /// <summary>
    /// 加密密碼
    /// </summary>
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = "BookVectorMVC_Salt_2024"; // 在實際應用中，應該為每個使用者生成唯一的鍵值
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        return Convert.ToBase64String(hashedBytes);
    }
}