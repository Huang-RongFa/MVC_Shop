using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MyWeb.Application.DTOs.Admin.Auth;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Application.Services.Auth;

/// <summary>
/// 後台登入流程 Service。
/// 負責密碼驗證、登入稽核、角色與權限 Claim 組裝；Controller 只負責 HTTP 與 Cookie 簽入。
/// </summary>
public sealed class AdminAuthService : IAdminAuthService
{
    private const string InvalidLoginCode = "AUTH_INVALID_CREDENTIALS";
    private readonly IUserAuthRepository _userAuthRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AdminAuthService(
        IUserAuthRepository userAuthRepository,
        IPasswordHasher<User> passwordHasher)
    {
        _userAuthRepository = userAuthRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AdminLoginServiceResult> LoginAsync(
        AdminLoginRequest request,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        // TODO: 正式上線需加入登入防暴力破解與 Rate Limiting，可依帳號、IP、UserAgent 記錄失敗次數。
        var account = request.Account.Trim();

        if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(request.Password))
        {
            return AdminLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        var user = await _userAuthRepository.GetAdminCandidateByAccountAsync(account, cancellationToken);
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

            return AdminLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        if (user.Status != "Active")
        {
            // 帳號存在但不可登入時仍寫入 UserLoginLogs，方便後台追蹤停用帳號嘗試登入。
            await _userAuthRepository.RecordLoginAsync(
                user.UserId,
                account,
                "Failed",
                $"UserStatus:{user.Status}",
                ipAddress,
                userAgent,
                cancellationToken);

            return AdminLoginServiceResult.Failure("AUTH_ACCOUNT_DISABLED", "此帳號目前不可登入。");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
        {
            // 密碼錯誤不回傳細節，避免帳號枚舉與登入流程資訊外洩。
            await _userAuthRepository.RecordLoginAsync(
                user.UserId,
                account,
                "Failed",
                "InvalidPassword",
                ipAddress,
                userAgent,
                cancellationToken);

            return AdminLoginServiceResult.Failure(InvalidLoginCode, "帳號或密碼錯誤。");
        }

        var roles = user.UserRoles
            .Where(userRole => !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role.RoleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role)
            .ToArray();

        var permissions = user.UserRoles
            .Where(userRole => !userRole.Role.IsDeleted)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.PermissionCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToArray();

        var response = new AdminAuthResponse
        {
            UserId = user.UserId,
            Account = user.Account,
            DisplayName = user.DisplayName,
            UserType = user.UserType,
            Roles = roles,
            Permissions = permissions
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

        return AdminLoginServiceResult.Success(response, BuildClaims(response));
    }

    public AdminAuthResponse BuildCurrentUser(ClaimsPrincipal user)
    {
        // 這裡只從 Cookie Claims 組裝目前登入狀態，不回查 Entity，避免把敏感欄位帶回 API。
        var roles = user.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(role => role)
            .ToArray();

        var permissions = user.FindAll("permission")
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permission => permission)
            .ToArray();

        var userIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = long.TryParse(userIdText, out var userId);

        return new AdminAuthResponse
        {
            UserId = userId,
            Account = user.FindFirstValue("account") ?? string.Empty,
            DisplayName = user.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            UserType = user.FindFirstValue("user_type") ?? string.Empty,
            Roles = roles,
            Permissions = permissions
        };
    }

    private static IReadOnlyCollection<Claim> BuildClaims(AdminAuthResponse user)
    {
        // TODO: 正式權限模型穩定後，可將 Claim Type 常數化，避免字串散落。
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new("account", user.Account),
            new("user_type", user.UserType)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim("permission", permission)));

        return claims;
    }
}
