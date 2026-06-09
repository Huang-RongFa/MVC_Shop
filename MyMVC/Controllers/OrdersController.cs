using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 前台會員訂單查詢頁面入口。
/// Service 層必須確認只能查詢目前會員自己的訂單，避免 Broken Object Level Authorization。
/// </summary>
[Authorize]
[Route("orders")]
public sealed class OrdersController : Controller
{
    private readonly IStorefrontPageService _storefrontPageService;

    public OrdersController(IStorefrontPageService storefrontPageService)
    {
        _storefrontPageService = storefrontPageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // TODO: 正式訂單列表需加入分頁、狀態篩選，且只回傳目前會員資料。
        var viewModel = await _storefrontPageService.GetOrderListAsync(User, cancellationToken);
        return View(viewModel);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        // TODO: Service 必須檢查訂單擁有者；找不到或無權限時應依規則回傳 404 或 403。
        var viewModel = await _storefrontPageService.GetOrderDetailAsync(User, id, cancellationToken);
        return View(viewModel);
    }
}
