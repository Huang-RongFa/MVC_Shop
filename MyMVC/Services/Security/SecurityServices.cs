using System.Security.Claims;

namespace MyWeb.Services.Security;

public interface ICurrentUserService
{
    long? UserId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
}

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(string permissionCode, CancellationToken cancellationToken = default);
}

public interface IClockService
{
    DateTime UtcNow { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long? UserId
    {
        get
        {
            var rawUserId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(rawUserId, out var userId) ? userId : null;
        }
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public IReadOnlyCollection<string> Roles =>
        _httpContextAccessor.HttpContext?.User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray() ?? [];

    public IReadOnlyCollection<string> Permissions =>
        _httpContextAccessor.HttpContext?.User.FindAll(AppAuthenticationDefaults.PermissionClaimType)
            .Select(claim => claim.Value)
            .ToArray() ?? [];
}

public class PermissionChecker : IPermissionChecker
{
    private readonly ICurrentUserService _currentUser;

    public PermissionChecker(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<bool> HasPermissionAsync(
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        var hasPermission = _currentUser.Permissions.Contains(
            permissionCode,
            StringComparer.OrdinalIgnoreCase);

        return Task.FromResult(hasPermission);
    }
}

public class ClockService : IClockService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
