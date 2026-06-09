using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 前台會員購物車頁面入口。
/// Controller 只回傳頁面，購物車項目、價格與優惠計算應由 CartService / PricingService 處理。
/// </summary>
[Authorize]
[Route("cart")]
public sealed class CartController : Controller
{
    private readonly IStorefrontPageService _storefrontPageService;

    public CartController(IStorefrontPageService storefrontPageService)
    {
        _storefrontPageService = storefrontPageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // TODO: 正式實作需確認目前登入會員，只查詢自己的購物車資料。
        var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
        return View(viewModel);
    }
}
