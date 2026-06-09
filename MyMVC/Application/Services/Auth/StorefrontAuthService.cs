using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MyWeb.Application.DTOs.Storefront.Auth;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Application.Services.Auth;

/// <summary>
/// 前台會員登入流程 Service。
/// 負責 Customer 身分驗證、登入稽核與 Claim 組裝；Controller 只處理 HTTP 與 Cookie 簽入。
/// </summary>
public sealed class StorefrontAuthService : IStorefrontAuthService
{
    private const string InvalidLoginCode = "AUTH_INVALID_CREDENTIALS";
    private readonly IUserAuthRepository _userAuthRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public StorefrontAuthService(
        IUserAuthRepository userAuthRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userAuthRepository = userAuthRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<StorefrontLoginServiceResult> LoginAsync(
        StorefrontLoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        // TODO: 正式上線需加入登入防暴力破解與 Rate Limiting，可依帳號、IP、UserAgent 記錄失敗次數。
        var account = request.Account.Trim();

        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(request.Password))
        {
            return StorefrontLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        var user = await _userAuthRepository.GetStorefrontCustomerByAccountAsync(account, cancellationToken);
        if (user is null)
        {
            await _userAuthRepository.RecordLoginAsync(
                userId: null,
                account: account,
                loginResult: "Failed",
                failureReason: "AccountNotFound",
                ipAddress: ipAddress,
                userAgent: userAgent,
                cancellationToken: cancellationToken);

            return StorefrontLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        if (user.Status != "Active")
        {
            await _userAuthRepository.RecordLoginAsync(
                user.UserId,
                account,
                "Failed",
                $"UserStatus:{user.Status}",
                ipAddress,
                userAgent,
                cancellationToken);

            return StorefrontLoginServiceResult.Failure("AUTH_ACCOUNT_DISABLED", "此帳號目前不可登入。");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            await _userAuthRepository.RecordLoginAsync(
                user.UserId,
                account,
                "Failed",
                "InvalidPassword",
                ipAddress,
                userAgent,
                cancellationToken);

            return StorefrontLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        var response = new StorefrontAuthResponse
        {
            UserId = user.UserId,
            Account = user.Account,
            DisplayName = user.DisplayName,
            UserType = user.UserType,
            Email = user.Email
        };

        // TODO: 密碼雜湊若回傳 SuccessRehashNeeded，應在交易安全範圍內更新 PasswordHash。
        await _userAuthRepository.RecordLoginAsync(
            user.UserId,
            account,
            "Success",
            failureReason: null,
            ipAddress,
            userAgent,
            cancellationToken);

        return StorefrontLoginServiceResult.Success(response, BuildClaims(response));
    }

    public StorefrontAuthResponse BuildCurrentUser(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = long.TryParse(userIdText, out var userId);

        return new StorefrontAuthResponse
        {
            UserId = userId,
            Account = user.FindFirstValue("account") ?? string.Empty,
            DisplayName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            UserType = user.FindFirstValue("user_type") ?? string.Empty,
            Email = user.FindFirstValue(ClaimTypes.Email)
        };
    }

    private static IReadOnlyCollection<Claim> BuildClaims(StorefrontAuthResponse user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new("account", user.Account),
            new("user_type", user.UserType),
            new(ClaimTypes.Role, "Customer")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }

        return claims;
    }
}
