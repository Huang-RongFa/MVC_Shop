using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs.Accounts;
using MyWeb.DTOs.Common;
using MyWeb.Extensions;
using MyWeb.Services.Accounts;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Route("api/storefront/auth")]
public class StorefrontAuthController : ApiControllerBase
{
    private readonly IAuthService _authService;

    public StorefrontAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserDto>>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            AuthClientType.Storefront,
            CreateRequestMetadata(),
            cancellationToken);

        if (!result.Succeeded)
        {
            return StatusCode(
                result.ErrorCode == ErrorCode.Forbidden
                    ? StatusCodes.Status403Forbidden
                    : StatusCodes.Status401Unauthorized,
                ApiResponse<AuthenticatedUserDto>.Failure(
                    result.ErrorCode,
                    result.ErrorMessage ?? "登入失敗。"));
        }

        await HttpContext.SignInAsync(
            AppAuthenticationDefaults.AuthenticationScheme,
            result.Principal!,
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

        return Ok(ApiResponse<AuthenticatedUserDto>.Success(result.User!));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken)
    {
        await _authService.RecordLogoutAsync(
            User.GetUserId(),
            CreateRequestMetadata(),
            cancellationToken);

        await HttpContext.SignOutAsync(AppAuthenticationDefaults.AuthenticationScheme);

        return Ok(ApiResponse.Success());
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthenticatedUserDto>>> Me(
        CancellationToken cancellationToken)
    {
        var user = await _authService.GetCurrentUserAsync(User, cancellationToken);

        if (user is null)
        {
            return Unauthorized(ApiResponse<AuthenticatedUserDto>.Failure(
                ErrorCode.Unauthorized,
                "登入狀態已失效。"));
        }

        return Ok(ApiResponse<AuthenticatedUserDto>.Success(user));
    }

    private RequestMetadata CreateRequestMetadata()
    {
        return new RequestMetadata
        {
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };
    }
}
