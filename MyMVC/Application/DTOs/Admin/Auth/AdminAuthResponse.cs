namespace MyWeb.Application.DTOs.Admin.Auth;

public sealed class AdminAuthResponse
{
    public long UserId { get; init; }
    public string Account { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserType { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
