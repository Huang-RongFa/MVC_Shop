namespace MyWeb.Application.DTOs.Admin.Auth;

public sealed class AdminAuthResponse
{
    public long UserId { get; init; }
    public string Account { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserType { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public IReadOnlyCollection<AdminRoleDescriptionDto> RoleDescriptions { get; init; } = Array.Empty<AdminRoleDescriptionDto>();
    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}

public sealed class AdminRoleDescriptionDto
{
    public string RoleCode { get; init; } = string.Empty;
    public string RoleName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}
