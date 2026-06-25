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
        string? origin,
        string? certification,
        int? minPrice,
        int? maxPrice,
        string? sort,
        string? view,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        // TODO: 正式商品列表應使用 ProductService 做資料庫端分頁與篩選，不應在頁面服務內使用展示集合篩選。
        var viewModel = await _storefrontPageService.GetProductListAsync(
            User,
            keyword,
            category,
            origin,
            certification,
            minPrice,
            maxPrice,
            sort,
            view,
            page,
            cancellationToken);

        return View(viewModel);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var viewModel = await _storefrontPageService.GetProductDetailAsync(User, id, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        return View(viewModel);
    }
}
