using System.ComponentModel.DataAnnotations;

namespace MyWeb.DTOs.Accounts;

public sealed class LoginRequest
{
    [Required]
    [MaxLength(100)]
    public string Account { get; init; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}

public sealed class AuthenticatedUserDto
{
    public long UserId { get; init; }
    public string Account { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string UserType { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public IReadOnlyCollection<string> Permissions { get; init; } = [];
}

public sealed class CsrfTokenResponse
{
    public string HeaderName { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
}
