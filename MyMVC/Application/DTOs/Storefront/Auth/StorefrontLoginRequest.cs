using System.ComponentModel.DataAnnotations;

namespace MyWeb.Application.DTOs.Storefront.Auth;

public sealed class StorefrontLoginRequest
{
    [Required]
    [StringLength(100)]
    public string Account { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }

    [StringLength(2048)]
    public string? ReturnUrl { get; init; }
}
