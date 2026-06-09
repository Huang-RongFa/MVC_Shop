using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Storefront.Auth;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 前台帳號頁面入口。
/// 登入流程已委派 StorefrontAuthService；註冊、忘記密碼與會員資料仍需由各自專責 Service 完成正式流程。
/// </summary>
[Route("account")]
public sealed class AccountController : Controller
{
    private readonly IAuthPageService _authPageService;
    private readonly IStorefrontAuthService _storefrontAuthService;
    private readonly IStorefrontPageService _storefrontPageService;

    public AccountController(
        IAuthPageService authPageService,
        IStorefrontAuthService storefrontAuthService,
        IStorefrontPageService storefrontPageService)
    {
        _authPageService = authPageService;
        _storefrontAuthService = storefrontAuthService;
        _storefrontPageService = storefrontPageService;
    }

    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        var safeReturnUrl = GetSafeReturnUrl(returnUrl);
        return View(_authPageService.GetStorefrontLoginPage(safeReturnUrl));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        StorefrontLoginRequest request,
        CancellationToken cancellationToken)
    {
        // 登入成功後只允許回跳站內路徑，避免開放重新導向漏洞；未提供時回到商品列表。
        var safeReturnUrl = GetSafeReturnUrl(request.ReturnUrl);
        var viewModel = _authPageService.GetStorefrontLoginPage(safeReturnUrl);

        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "請輸入有效的帳號與密碼。");
            return View(viewModel);
        }

        var result = await _storefrontAuthService.LoginAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        if (!result.Succeeded || result.User is null)
        {
            // 登入失敗不透露帳號是否存在、密碼是否錯誤或內部狀態，降低帳號枚舉風險。
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "帳號或密碼錯誤。");

            return View(viewModel);
        }

        var identity = new ClaimsIdentity(
            result.Claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

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

        return LocalRedirect(safeReturnUrl);
    }

    [AllowAnonymous]
    [HttpGet("register")]
    public IActionResult Register()
    {
        // TODO: 註冊流程尚未實作；需加入輸入驗證、密碼雜湊、Email/手機驗證與 UserLoginLogs。
        return View(_authPageService.GetRegisterPage());
    }

    [Authorize]
    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword()
    {
        // TODO: 忘記密碼流程尚未實作；需使用一次性短效 token，且不可透露帳號是否存在。
        return View(_authPageService.GetForgotPasswordPage());
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        // TODO: 正式會員資料需由 MemberService 查詢目前會員，並避免回傳敏感欄位。
        var viewModel = await _storefrontPageService.GetMemberProfileAsync(User, cancellationToken);
        return View(viewModel);
    }

    private string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return "/products";
        }

        return Url.IsLocalUrl(returnUrl) ? returnUrl : "/products";
    }
}
