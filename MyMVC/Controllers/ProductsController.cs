using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;

namespace MyWeb.Controllers;

/// <summary>
/// 前台商品瀏覽頁面入口。
/// 匿名使用者可瀏覽商品；價格、庫存與可購買狀態仍需由正式 ProductService 提供。
/// </summary>
[Route("products")]
public sealed class ProductsController : Controller
{
    private readonly IStorefrontPageService _storefrontPageService;

    public ProductsController(IStorefrontPageService storefrontPageService)
    {
        _storefrontPageService = storefrontPageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? keyword,
        string? category,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        // TODO: 正式商品列表應使用 ProductService 做資料庫端分頁與篩選，不應在頁面服務內使用展示集合篩選。
        var viewModel = await _storefrontPageService.GetProductListAsync(
            User,
            keyword,
            category,
            page,
            cancellationToken);

        return View(viewModel);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        // TODO: 找不到商品時應由 Service 回傳明確結果，Controller 轉成 NotFound()。
        var viewModel = await _storefrontPageService.GetProductDetailAsync(User, id, cancellationToken);
        return View(viewModel);
    }
}
