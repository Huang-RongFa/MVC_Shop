using System.Security.Claims;
using MyWeb.Application.DTOs.Storefront.Auth;

namespace MyWeb.Application.Interfaces.Auth;

/// <summary>
/// 前台會員登入與目前使用者狀態服務。
/// 實作需避免回傳 PasswordHash、SecurityStamp 或任何內部安全欄位。
/// </summary>
public interface IStorefrontAuthService
{
    /// <summary>驗證 Customer 帳號密碼並建立前台 Claims；失敗時需寫入登入紀錄。</summary>
    Task<StorefrontLoginServiceResult> LoginAsync(
        StorefrontLoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);

    /// <summary>由目前 Cookie Claims 組裝可回傳前台的登入狀態 DTO。</summary>
    StorefrontAuthResponse BuildCurrentUser(ClaimsPrincipal user);
}

/// <summary>
/// Service 層登入結果，讓 Controller 可依狀態決定 Cookie 簽入與頁面導向。
/// </summary>
public sealed class StorefrontLoginServiceResult
{
    public bool Succeeded { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public StorefrontAuthResponse? User { get; init; }

    public IReadOnlyCollection<Claim> Claims { get; init; } = Array.Empty<Claim>();

    public static StorefrontLoginServiceResult Success(
        StorefrontAuthResponse user,
        IReadOnlyCollection<Claim> claims)
    {
        return new StorefrontLoginServiceResult
        {
            Succeeded = true,
            User = user,
            Claims = claims
        };
    }

    public static StorefrontLoginServiceResult Failure(string code, string message)
    {
        return new StorefrontLoginServiceResult
        {
            Succeeded = false,
            ErrorCode = code,
            ErrorMessage = message
        };
    }
}
