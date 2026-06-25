using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Storefront.Cart;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Controllers;

/// <summary>
/// 前台會員購物車頁面與操作入口。
/// Controller 僅負責 HTTP request/response，購物車規則仍由 CartService 處理。
/// </summary>
[Authorize]
[Route("cart")]
public sealed class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IStorefrontPageService _storefrontPageService;

    public CartController(
        ICartService cartService,
        IStorefrontPageService storefrontPageService)
    {
        _cartService = cartService;
        _storefrontPageService = storefrontPageService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
        return View(viewModel);
    }

    [HttpGet("drawer")]
    public async Task<IActionResult> Drawer(CancellationToken cancellationToken)
    {
        var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
        return Json(ToCartDrawerPayload(viewModel));
    }

    [HttpPost("items")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.AddItemAsync(User, request, cancellationToken);
        if (IsAjaxRequest())
        {
            var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
            return Json(new
            {
                result.Succeeded,
                result.Message,
                request.ProductId,
                QuantityAdded = request.Quantity,
                Cart = ToCartDrawerPayload(viewModel)
            });
        }

        SetCartMessage(result);

        return RedirectAfterCartAction(request.ReturnUrl);
    }

    [HttpPost("items/{cartItemId:long}/quantity")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(
        long cartItemId,
        int quantity,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.UpdateQuantityAsync(
            User,
            cartItemId,
            quantity,
            cancellationToken);

        if (!result.Succeeded)
        {
            SetCartMessage(result);
        }

        if (IsAjaxRequest())
        {
            var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
            return Json(new
            {
                result.Succeeded,
                result.Message,
                Cart = ToCartDrawerPayload(viewModel)
            });
        }

        return RedirectAfterCartAction(returnUrl);
    }

    [HttpPost("items/{cartItemId:long}/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(
        long cartItemId,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.RemoveItemAsync(
            User,
            cartItemId,
            cancellationToken);

        SetCartMessage(result);

        if (IsAjaxRequest())
        {
            var viewModel = await _storefrontPageService.GetCartAsync(User, cancellationToken);
            return Json(new
            {
                result.Succeeded,
                result.Message,
                Cart = ToCartDrawerPayload(viewModel)
            });
        }

        return RedirectAfterCartAction(returnUrl);
    }

    [HttpPost("coupon-preview")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PreviewCoupon(
        string? couponCode,
        CancellationToken cancellationToken)
    {
        var result = await _cartService.PreviewCouponAsync(
            User,
            couponCode,
            cancellationToken);

        return Json(result);
    }

    private void SetCartMessage(CartOperationResult result)
    {
        TempData[result.Succeeded ? "CartSuccessMessage" : "CartErrorMessage"] = result.Message;
    }

    private IActionResult RedirectAfterCartAction(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool IsAjaxRequest()
    {
        return string.Equals(
            Request.Headers["X-Requested-With"].ToString(),
            "XMLHttpRequest",
            StringComparison.OrdinalIgnoreCase);
    }

    private static object ToCartDrawerPayload(CartViewModel viewModel)
    {
        return new
        {
            ItemCount = viewModel.Items.Sum(item => item.Quantity),
            viewModel.SubtotalText,
            viewModel.DiscountTotalText,
            viewModel.EstimatedTotalText,
            viewModel.CanCheckout,
            Items = viewModel.Items.Select(item => new
            {
                item.CartItemId,
                item.ProductId,
                item.SkuId,
                item.ProductName,
                item.SkuName,
                item.Quantity,
                item.UnitPriceText,
                item.LineTotalText,
                item.StockStatusText
            }).ToArray()
        };
    }
}
