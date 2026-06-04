using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.DTOs.Accounts;
using MyWeb.DTOs.Common;
using MyWeb.Repositories;
using MyWeb.Repositories.Accounts;
using MyWeb.Services.Security;

namespace MyWeb.Services.Accounts;

public interface IAuthService
{
    Task<AuthSignInResult> LoginAsync(
        LoginRequest request,
        AuthClientType clientType,
        RequestMetadata metadata,
        CancellationToken cancellationToken = default);

    Task<AuthenticatedUserDto?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);

    Task RecordLogoutAsync(
        long? userId,
        RequestMetadata metadata,
        CancellationToken cancellationToken = default);
}

public interface IUserService
{
}

public interface IRoleService
{
}

public interface IPermissionService
{
}

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IClockService _clock;

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        IClockService clock)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<AuthSignInResult> LoginAsync(
        LoginRequest request,
        AuthClientType clientType,
        RequestMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        var normalizedAccount = request.Account.Trim();
        var user = await _userRepository.GetByAccountForLoginAsync(
            normalizedAccount,
            cancellationToken);

        if (user is null)
        {
            await RecordLoginFailureAsync(
                null,
                normalizedAccount,
                "帳號或密碼錯誤。",
                metadata,
                cancellationToken);

            return AuthSignInResult.Failure(
                ErrorCode.Unauthorized,
                "帳號或密碼錯誤。");
        }

        if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            await RecordLoginFailureAsync(
                user.UserId,
                normalizedAccount,
                "帳號狀態不可登入。",
                metadata,
                cancellationToken);

            return AuthSignInResult.Failure(
                ErrorCode.Forbidden,
                "帳號目前不可登入。");
        }

        if (!IsAllowedClientUserType(user.UserType, clientType))
        {
            await RecordLoginFailureAsync(
                user.UserId,
                normalizedAccount,
                "登入入口與帳號身分不符。",
                metadata,
                cancellationToken);

            return AuthSignInResult.Failure(
                ErrorCode.Forbidden,
                "此帳號不可由目前入口登入。");
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            await RecordLoginFailureAsync(
                user.UserId,
                normalizedAccount,
                "帳號或密碼錯誤。",
                metadata,
                cancellationToken);

            return AuthSignInResult.Failure(
                ErrorCode.Unauthorized,
                "帳號或密碼錯誤。");
        }

        user.LastLoginAt = _clock.UtcNow;
        _userRepository.AddLoginLog(CreateLoginLog(
            user.UserId,
            normalizedAccount,
            "Success",
            null,
            metadata));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var userDto = CreateUserDto(user);
        var principal = CreatePrincipal(userDto);

        return AuthSignInResult.Success(userDto, principal);
    }

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default)
    {
        var rawUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!long.TryParse(rawUserId, out var userId))
        {
            return null;
        }

        var user = await _userRepository.GetByIdForClaimsAsync(userId, cancellationToken);

        if (user is null ||
            !string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return CreateUserDto(user);
    }

    public async Task RecordLogoutAsync(
        long? userId,
        RequestMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        _userRepository.AddLoginLog(CreateLoginLog(
            userId,
            null,
            "Logout",
            null,
            metadata));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task RecordLoginFailureAsync(
        long? userId,
        string account,
        string failureReason,
        RequestMetadata metadata,
        CancellationToken cancellationToken)
    {
        _userRepository.AddLoginLog(CreateLoginLog(
            userId,
            account,
            "Failed",
            failureReason,
            metadata));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private UserLoginLog CreateLoginLog(
        long? userId,
        string? account,
        string result,
        string? failureReason,
        RequestMetadata metadata)
    {
        return new UserLoginLog
        {
            UserId = userId,
            Account = account,
            LoginResult = result,
            FailureReason = failureReason,
            IpAddress = metadata.IpAddress,
            UserAgent = metadata.UserAgent,
            LoginAt = _clock.UtcNow
        };
    }

    private static bool IsAllowedClientUserType(
        string userType,
        AuthClientType clientType)
    {
        return clientType switch
        {
            AuthClientType.Storefront =>
                string.Equals(userType, "Customer", StringComparison.OrdinalIgnoreCase),
            AuthClientType.Admin =>
                AuthPolicies.AdminUserTypes.Contains(userType, StringComparer.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static AuthenticatedUserDto CreateUserDto(User user)
    {
        var roles = user.UserRoles
            .Where(userRole => !userRole.Role.IsDeleted)
            .Select(userRole => userRole.Role.RoleCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(roleCode => roleCode)
            .ToArray();

        var permissions = user.UserRoles
            .Where(userRole => !userRole.Role.IsDeleted)
            .SelectMany(userRole => userRole.Role.RolePermissions)
            .Select(rolePermission => rolePermission.Permission.PermissionCode)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(permissionCode => permissionCode)
            .ToArray();

        return new AuthenticatedUserDto
        {
            UserId = user.UserId,
            Account = user.Account,
            DisplayName = user.DisplayName,
            UserType = user.UserType,
            Roles = roles,
            Permissions = permissions
        };
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.Account),
            new("display_name", user.DisplayName),
            new(AppAuthenticationDefaults.UserTypeClaimType, user.UserType)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission =>
            new Claim(AppAuthenticationDefaults.PermissionClaimType, permission)));

        var identity = new ClaimsIdentity(
            claims,
            AppAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}

public class UserService : IUserService
{
}

public class RoleService : IRoleService
{
}

public class PermissionService : IPermissionService
{
}

public enum AuthClientType
{
    Storefront = 1,
    Admin = 2
}

public sealed class RequestMetadata
{
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}

public sealed class AuthSignInResult
{
    private AuthSignInResult(
        bool succeeded,
        ErrorCode errorCode,
        string? errorMessage,
        AuthenticatedUserDto? user,
        ClaimsPrincipal? principal)
    {
        Succeeded = succeeded;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        User = user;
        Principal = principal;
    }

    public bool Succeeded { get; }
    public ErrorCode ErrorCode { get; }
    public string? ErrorMessage { get; }
    public AuthenticatedUserDto? User { get; }
    public ClaimsPrincipal? Principal { get; }

    public static AuthSignInResult Success(
        AuthenticatedUserDto user,
        ClaimsPrincipal principal)
    {
        return new AuthSignInResult(true, ErrorCode.None, null, user, principal);
    }

    public static AuthSignInResult Failure(ErrorCode errorCode, string message)
    {
        return new AuthSignInResult(false, errorCode, message, null, null);
    }
}
