using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.Auth;

namespace MyWeb.Application.Interfaces.Auth;

/// <summary>
/// 後台登入與目前使用者狀態服務。
/// 實作需避免回傳 PasswordHash、SecurityStamp 或任何內部安全欄位。
/// </summary>
public interface IAdminAuthService
{
    /// <summary>驗證後台帳號密碼並建立角色、權限 Claims；失敗時需寫入登入紀錄。</summary>
    Task<AdminLoginServiceResult> LoginAsync(
        AdminLoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    /// <summary>由目前 Cookie Claims 組裝可回傳前端的登入狀態 DTO。</summary>
    AdminAuthResponse BuildCurrentUser(ClaimsPrincipal user);
}

/// <summary>
/// Service 層登入結果，讓 Controller 可依狀態決定 HTTP Status Code 與 Cookie 簽入。
/// </summary>
public sealed class AdminLoginServiceResult
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
    public string? ErrorMessage { get; init; }
    public AdminAuthResponse? User { get; init; }
    public IReadOnlyCollection<Claim> Claims { get; init; } = Array.Empty<Claim>();

    /// <summary>建立登入成功結果；User 僅能包含可公開給後台前端的欄位。</summary>
    public static AdminLoginServiceResult Success(AdminAuthResponse user, IReadOnlyCollection<Claim> claims)
    {
        return new AdminLoginServiceResult
        {
            Succeeded = true,
            User = user,
            Claims = claims
        };
    }

    /// <summary>建立登入失敗結果；錯誤訊息不可暴露帳號存在與否或內部例外。</summary>
    public static AdminLoginServiceResult Failure(string code, string message)
    {
        return new AdminLoginServiceResult
        {
            Succeeded = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
