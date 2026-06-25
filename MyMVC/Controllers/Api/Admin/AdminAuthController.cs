using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Admin.Auth;
using MyWeb.Application.DTOs.Common;
using MyWeb.Application.Interfaces.Auth;

namespace MyWeb.Controllers.Api.Admin;

/// <summary>
/// 後台登入 API。
/// Controller 負責 HTTP 狀態、Cookie 簽入/登出與 DTO 回傳；密碼與權限規則交由 AdminAuthService。
/// </summary>
[ApiController]
[Route("api/admin/auth")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly IAdminAuthService _adminAuthService;

    public AdminAuthController(IAdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AdminAuthResponse>>> Login(
        [FromBody] AdminLoginRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            // 基本輸入錯誤回 400；帳密錯誤則由 Service 統一回 Unauthorized，避免帳號枚舉。
            return BadRequest(ApiResponse.Failure("AUTH_INVALID_INPUT", "請輸入有效的帳號與密碼。"));
        }

        var result = await _adminAuthService.LoginAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            // 登入失敗不透露帳號是否存在、密碼是否錯誤或帳號內部狀態。
            return Unauthorized(ApiResponse.Failure(
                result.ErrorCode ?? "AUTH_LOGIN_FAILED",
                result.ErrorMessage ?? "登入失敗。"));
        }

        var identity = new ClaimsIdentity(
            result.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        // TODO: 正式上線需搭配 Rate Limiting、CSRF、Cookie Secure 與可信任網域設定檢查。
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            });

        return Ok(ApiResponse<AdminAuthResponse>.Success(result.User));
    }

    [HttpGet("me")]
    public ActionResult<ApiResponse<AdminAuthResponse>> Me()
    {
        // 回傳目前登入狀態 DTO，不回傳 User Entity，避免敏感欄位外洩。
        return Ok(ApiResponse<AdminAuthResponse>.Success(_adminAuthService.BuildCurrentUser(User)));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout()
    {
        // TODO: 若未來加入 Session / Refresh Token / 裝置管理，登出時需同步撤銷伺服器端狀態。
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(ApiResponse.Success());
    }
}
