using System.Security.Claims;
using MyWeb.Models.ViewModels.Auth;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Interfaces.Pages;

/// <summary>
/// 前台 MVC 頁面資料組裝介面。
/// TODO: 正式 API / Vue App 完成後，這裡可逐步改由 ProductService、CartService、OrderService 等業務 Service 提供資料。
/// </summary>
public interface IStorefrontPageService
{
    /// <summary>取得前台首頁資料；匿名可瀏覽，但購物車數量需依登入會員計算。</summary>
    Task<StorefrontHomeViewModel> GetHomeAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得商品列表；正式實作需在資料庫端分頁、篩選與排序。</summary>
    Task<ProductListViewModel> GetProductListAsync(
        ClaimsPrincipal user,
        string? keyword,
        string? category,
        string? origin,
        string? certification,
        int? minPrice,
        int? maxPrice,
        string? sort,
        string? view,
        int page,
        CancellationToken cancellationToken);

    /// <summary>取得商品詳細；找不到商品時應讓 Controller 回傳 404。</summary>
    Task<ProductDetailViewModel?> GetProductDetailAsync(
        ClaimsPrincipal user,
        int productId,
        CancellationToken cancellationToken);

    /// <summary>取得農夫故事頁資料；匿名可瀏覽，篩選僅影響展示資料。</summary>
    Task<FarmerStoryPageViewModel> GetFarmerStoriesAsync(
        string? filter,
        CancellationToken cancellationToken);

    /// <summary>取得目前會員購物車；正式實作需重新驗價與確認 SKU 可售狀態。</summary>
    Task<CartViewModel> GetCartAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得結帳頁資料；建立訂單時仍必須在 Service 交易中重新檢查庫存與優惠。</summary>
    Task<CheckoutViewModel> GetCheckoutAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得結帳完成頁資料；正式實作需確認訂單屬於目前會員。</summary>
    Task<CheckoutCompleteViewModel> GetCheckoutCompleteAsync(
        ClaimsPrincipal user,
        string? orderNumber,
        CancellationToken cancellationToken);

    /// <summary>取得目前會員訂單列表；不可回傳其他會員訂單。</summary>
    Task<OrderListViewModel> GetOrderListAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得目前會員訂單明細；Service 層需檢查資料擁有者。</summary>
    Task<OrderDetailViewModel> GetOrderDetailAsync(
        ClaimsPrincipal user,
        int orderId,
        CancellationToken cancellationToken);

    /// <summary>取得會員資料；不可包含密碼雜湊、SecurityStamp 或內部稽核欄位。</summary>
    Task<MemberProfileViewModel> GetMemberProfileAsync(ClaimsPrincipal user, CancellationToken cancellationToken);
}
