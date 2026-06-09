using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 後台 Razor 頁面入口。
/// Controller 僅負責路由、授權入口與 View 回傳；正式模組資料與權限判斷需放在 Service 層。
/// </summary>
[Authorize]
[Route("admin")]
public sealed class AdminController : Controller
{
    private readonly IAdminPageService _adminPageService;
    private readonly IAuthPageService _authPageService;

    public AdminController(
        IAdminPageService adminPageService,
        IAuthPageService authPageService)
    {
        _adminPageService = adminPageService;
        _authPageService = authPageService;
    }

    [HttpGet("")]
    [HttpHead("")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _adminPageService.GetDashboardAsync(User, cancellationToken);
        return View(viewModel);
    }

    [AllowAnonymous]
    [HttpGet("login")]
    [HttpHead("login")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Login()
    {
        return View(_authPageService.GetAdminLoginPage());
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> Analytics(CancellationToken cancellationToken)
    {
        // TODO: 正式上線前需增加 Analytics.Read 或等效 Policy，避免所有已登入後台帳號看到敏感營運指標。
        var viewModel = await _adminPageService.GetAnalyticsAsync(User, cancellationToken);
        return View(viewModel);
    }

    [HttpGet("users")]
    public Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        return Module("users", cancellationToken);
    }

    [HttpGet("roles")]
    public Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        return Module("roles", cancellationToken);
    }

    [HttpGet("products")]
    public Task<IActionResult> Products(CancellationToken cancellationToken)
    {
        return Module("products", cancellationToken);
    }

    [HttpGet("products/edit")]
    public Task<IActionResult> ProductEdit(CancellationToken cancellationToken)
    {
        return Module("product-edit", cancellationToken);
    }

    [HttpGet("products/skus")]
    public Task<IActionResult> ProductSkus(CancellationToken cancellationToken)
    {
        return Module("product-skus", cancellationToken);
    }

    [HttpGet("inventory")]
    public Task<IActionResult> Inventory(CancellationToken cancellationToken)
    {
        return Module("inventory", cancellationToken);
    }

    [HttpGet("orders")]
    public Task<IActionResult> Orders(CancellationToken cancellationToken)
    {
        return Module("orders", cancellationToken);
    }

    [HttpGet("orders/details")]
    public Task<IActionResult> OrderDetails(CancellationToken cancellationToken)
    {
        return Module("order-details", cancellationToken);
    }

    [HttpGet("payments")]
    public Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        return Module("payments", cancellationToken);
    }

    [HttpGet("shipments")]
    public Task<IActionResult> Shipments(CancellationToken cancellationToken)
    {
        return Module("shipments", cancellationToken);
    }

    [HttpGet("refunds")]
    public Task<IActionResult> Refunds(CancellationToken cancellationToken)
    {
        return Module("refunds", cancellationToken);
    }

    [HttpGet("coupons")]
    public Task<IActionResult> Coupons(CancellationToken cancellationToken)
    {
        return Module("coupons", cancellationToken);
    }

    [HttpGet("promotions")]
    public Task<IActionResult> Promotions(CancellationToken cancellationToken)
    {
        return Module("promotions", cancellationToken);
    }

    [HttpGet("inbox")]
    public Task<IActionResult> Inbox(CancellationToken cancellationToken)
    {
        return Module("inbox", cancellationToken);
    }

    [HttpGet("audit-logs")]
    public Task<IActionResult> AuditLogs(CancellationToken cancellationToken)
    {
        return Module("audit-logs", cancellationToken);
    }

    [HttpGet("settings")]
    public Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return Module("settings", cancellationToken);
    }

    private async Task<IActionResult> Module(string moduleKey, CancellationToken cancellationToken)
    {
        // TODO: 依 moduleKey 對應權限 Policy，並在 AdminPageService / 專責 Service 做第二層 Permission 檢查。
        var viewModel = await _adminPageService.GetModulePageAsync(User, moduleKey, cancellationToken);
        return View(viewModel);
    }
}
