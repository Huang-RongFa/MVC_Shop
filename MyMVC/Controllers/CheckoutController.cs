using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 前台會員結帳頁面入口。
/// 建立訂單、保留庫存、折扣計算與付款建立都應在 Service 交易中完成。
/// </summary>
[Authorize]
[Route("checkout")]
public sealed class CheckoutController : Controller
{
    private readonly IStorefrontPageService _storefrontPageService;

    public CheckoutController(IStorefrontPageService storefrontPageService)
    {
        _storefrontPageService = storefrontPageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // TODO: 正式結帳頁需由 CheckoutService 重新驗證購物車、價格、優惠、庫存與配送條件。
        var viewModel = await _storefrontPageService.GetCheckoutAsync(User, cancellationToken);
        return View(viewModel);
    }

    [HttpGet("complete")]
    public async Task<IActionResult> Complete(string? orderNumber, CancellationToken cancellationToken)
    {
        // TODO: 不可只依 orderNumber query string 顯示訂單；需以目前會員身分查詢並確認資料擁有者。
        var viewModel = await _storefrontPageService.GetCheckoutCompleteAsync(
            User,
            orderNumber,
            cancellationToken);

        return View(viewModel);
    }
}
