using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Application.Security;

namespace MyWeb.Controllers;

/// <summary>
/// 後台 Razor 頁面入口。
/// Controller 僅負責路由、授權入口與 View 回傳；正式模組資料與權限判斷需放在 Service 層。
/// </summary>
[Authorize(Policy = "AdminOnly")]
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
    [Authorize(Policy = AdminPermissionCodes.DashboardRead)]
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
    [Authorize(Policy = "Analytics.Read")]
    public async Task<IActionResult> Analytics(CancellationToken cancellationToken)
    {
        try
        {
            var viewModel = await _adminPageService.GetAnalyticsAsync(User, cancellationToken);
            return View(viewModel);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("users")]
    [Authorize(Policy = "Users.Manage")]
    public Task<IActionResult> Users(CancellationToken cancellationToken)
    {
        return Module("users", cancellationToken);
    }

    [HttpGet("roles")]
    [Authorize(Policy = "Roles.Manage")]
    public Task<IActionResult> Roles(CancellationToken cancellationToken)
    {
        return Module("roles", cancellationToken);
    }

    [HttpGet("products")]
    [Authorize(Policy = "Products.Read")]
    public Task<IActionResult> Products(CancellationToken cancellationToken)
    {
        return Module("products", cancellationToken);
    }

    [HttpGet("products/edit")]
    [Authorize(Policy = "Products.Write")]
    public Task<IActionResult> ProductEdit(CancellationToken cancellationToken)
    {
        return Module("product-edit", cancellationToken);
    }

    [HttpGet("products/skus")]
    [Authorize(Policy = "Products.Write")]
    public Task<IActionResult> ProductSkus(CancellationToken cancellationToken)
    {
        return Module("product-skus", cancellationToken);
    }

    [HttpGet("inventory")]
    [Authorize(Policy = "Inventory.Read")]
    public Task<IActionResult> Inventory(CancellationToken cancellationToken)
    {
        return Module("inventory", cancellationToken);
    }

    [HttpGet("orders")]
    [Authorize(Policy = "Orders.Read")]
    public Task<IActionResult> Orders(CancellationToken cancellationToken)
    {
        return Module("orders", cancellationToken);
    }

    [HttpGet("orders/details")]
    [Authorize(Policy = "Orders.Read")]
    public Task<IActionResult> OrderDetails(CancellationToken cancellationToken)
    {
        return Module("order-details", cancellationToken);
    }

    [HttpGet("payments")]
    [Authorize(Policy = "Payments.Read")]
    public Task<IActionResult> Payments(CancellationToken cancellationToken)
    {
        return Module("payments", cancellationToken);
    }

    [HttpGet("shipments")]
    [Authorize(Policy = "Shipments.Manage")]
    public Task<IActionResult> Shipments(CancellationToken cancellationToken)
    {
        return Module("shipments", cancellationToken);
    }

    [HttpGet("refunds")]
    [Authorize(Policy = "Refunds.Manage")]
    public Task<IActionResult> Refunds(CancellationToken cancellationToken)
    {
        return Module("refunds", cancellationToken);
    }

    [HttpGet("coupons")]
    [Authorize(Policy = "Coupons.Manage")]
    public Task<IActionResult> Coupons(CancellationToken cancellationToken)
    {
        return Module("coupons", cancellationToken);
    }

    [HttpGet("promotions")]
    [Authorize(Policy = "Promotions.Manage")]
    public Task<IActionResult> Promotions(CancellationToken cancellationToken)
    {
        return Module("promotions", cancellationToken);
    }

    [HttpGet("inbox")]
    [Authorize(Policy = "Messages.Read")]
    public Task<IActionResult> Inbox(CancellationToken cancellationToken)
    {
        return Module("inbox", cancellationToken);
    }

    [HttpGet("audit-logs")]
    [Authorize(Policy = "AuditLogs.Read")]
    public Task<IActionResult> AuditLogs(CancellationToken cancellationToken)
    {
        return Module("audit-logs", cancellationToken);
    }

    [HttpGet("settings")]
    [Authorize(Policy = "Settings.Manage")]
    public Task<IActionResult> Settings(CancellationToken cancellationToken)
    {
        return Module("settings", cancellationToken);
    }

    private async Task<IActionResult> Module(string moduleKey, CancellationToken cancellationToken)
    {
        try
        {
            var viewModel = await _adminPageService.GetModulePageAsync(User, moduleKey, cancellationToken);
            return View(viewModel);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
