using System.ComponentModel.DataAnnotations;

namespace MyWeb.Application.DTOs.Admin.Auth;

public sealed class AdminLoginRequest
{
    [Required]
    [StringLength(100)]
    public string Account { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 6)]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}
