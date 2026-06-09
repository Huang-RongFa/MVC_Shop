using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models;

namespace MyWeb.Controllers;

/// <summary>
/// 前台公開頁面入口。
/// 首頁與關於頁可匿名瀏覽，不可顯示後台營收、分析或管理資料。
/// </summary>
public class HomeController : Controller
{
    private readonly IStorefrontPageService _storefrontPageService;

    public HomeController(IStorefrontPageService storefrontPageService)
    {
        _storefrontPageService = storefrontPageService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        // TODO: 首頁正式資料需由 StorefrontPageService 串接商品、分類與促銷 Service。
        var viewModel = await _storefrontPageService.GetHomeAsync(User, cancellationToken);
        return View(viewModel);
    }

    public async Task<IActionResult> About(CancellationToken cancellationToken)
    {
        // TODO: 關於頁目前共用首頁 ViewModel；正式可拆成品牌內容 / SEO ViewModel。
        var viewModel = await _storefrontPageService.GetHomeAsync(User, cancellationToken);
        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
