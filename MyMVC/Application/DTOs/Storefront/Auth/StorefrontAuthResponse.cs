namespace MyWeb.Application.DTOs.Storefront.Auth;

public sealed class StorefrontAuthResponse
{
    public long UserId { get; init; }

    public string Account { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string UserType { get; init; } = string.Empty;

    public string? Email { get; init; }
}
