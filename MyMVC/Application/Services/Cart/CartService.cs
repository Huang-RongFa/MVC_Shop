using System.Globalization;
using System.Security.Claims;
using MyWeb.Application.DTOs.Storefront.Cart;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Domain.Entities.Orders;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Services.Cart;

/// <summary>
/// 前台會員購物車商業流程。
/// 此層負責確認會員身分、重新查詢可購買 SKU、檢查庫存與組裝 ViewModel；
/// Controller 不直接操作 DbContext，也不信任前端傳入的價格或庫存狀態。
/// </summary>
public sealed class CartService : ICartService
{
    private const int MaxCartItemQuantity = 99;
    private static readonly CultureInfo TaiwanCulture = CultureInfo.GetCultureInfo("zh-TW");
    private static readonly IReadOnlyList<CartCouponDefinition> CouponDefinitions =
    [
        new("WELCOME100", "新會員滿額折抵", "滿 NT$ 500 折 NT$ 100", "Amount", 100m, 500m, null),
        new("FRESH10", "新鮮食材 9 折", "滿 NT$ 300 享 9 折，最高折 NT$ 150", "Percent", 10m, 300m, 150m)
    ];

    private readonly ICartRepository _cartRepository;

    public CartService(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartOperationResult> AddItemAsync(
        ClaimsPrincipal user,
        AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        // 前台購物車只允許 Customer 操作，避免後台管理員 Cookie 誤用前台購買流程。
        if (!TryGetUserId(user, out var userId))
        {
            return CartOperationResult.Failure("請先登入會員後再加入購物車。");
        }

        if (request.ProductId <= 0)
        {
            return CartOperationResult.Failure("商品資料不正確。");
        }

        if (request.Quantity is < 1 or > MaxCartItemQuantity)
        {
            return CartOperationResult.Failure($"購買數量必須介於 1 到 {MaxCartItemQuantity} 之間。");
        }

        var sku = await _cartRepository.GetDefaultPurchasableSkuAsync(
            request.ProductId,
            cancellationToken);

        // SKU 與售價一律從資料庫重查，不能使用前端送來的商品名稱、價格或狀態。
        if (sku is null)
        {
            return CartOperationResult.Failure("商品規格不存在、已下架或暫時無法購買。");
        }

        var availableQuantity = await _cartRepository.GetAvailableQuantityAsync(
            sku.SkuId,
            cancellationToken);

        if (availableQuantity <= 0)
        {
            return CartOperationResult.Failure("此商品目前沒有可購買庫存。");
        }

        var cart = await _cartRepository.GetActiveCartForUpdateAsync(userId, cancellationToken);
        if (cart is null)
        {
            // 只有第一次加入商品時才建立 Active Cart，避免空購物車資料提前膨脹。
            cart = new ShoppingCart
            {
                UserId = userId,
                CartStatus = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _cartRepository.AddCart(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(item => item.SkuId == sku.SkuId);
        var nextQuantity = request.Quantity + (existingItem?.Quantity ?? 0);
        var quantityLimit = Math.Min(MaxCartItemQuantity, availableQuantity);
        // 使用現有購物車數量加總檢查，避免重複加入同商品時繞過單品數量上限。
        if (nextQuantity > quantityLimit)
        {
            return CartOperationResult.Failure($"此商品目前最多可購買 {quantityLimit} 件。");
        }

        if (existingItem is null)
        {
            cart.Items.Add(new ShoppingCartItem
            {
                SkuId = sku.SkuId,
                Quantity = request.Quantity,
                UnitPrice = sku.SalePrice,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existingItem.Quantity = nextQuantity;
            existingItem.UnitPrice = sku.SalePrice;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return CartOperationResult.Success("已加入購物車。");
    }

    public async Task<CartOperationResult> UpdateQuantityAsync(
        ClaimsPrincipal user,
        long cartItemId,
        int quantity,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return CartOperationResult.Failure("請先登入會員後再調整購物車。");
        }

        if (quantity is < 1 or > MaxCartItemQuantity)
        {
            return CartOperationResult.Failure($"購買數量必須介於 1 到 {MaxCartItemQuantity} 之間。");
        }

        var item = await _cartRepository.GetCartItemForUpdateAsync(
            userId,
            cartItemId,
            cancellationToken);

        // Repository 查詢已限制 Cart.UserId，找不到即視為不存在或非本人資料。
        if (item is null)
        {
            return CartOperationResult.Failure("找不到可調整的購物車項目。");
        }

        var availableQuantity = await _cartRepository.GetAvailableQuantityAsync(
            item.SkuId,
            cancellationToken);

        if (availableQuantity <= 0)
        {
            return CartOperationResult.Failure("此商品目前沒有可購買庫存。");
        }

        var quantityLimit = Math.Min(MaxCartItemQuantity, availableQuantity);
        if (quantity > quantityLimit)
        {
            return CartOperationResult.Failure($"此商品目前最多可購買 {quantityLimit} 件。");
        }

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        item.Cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.SaveChangesAsync(cancellationToken);

        return CartOperationResult.Success("購物車數量已更新。");
    }

    public async Task<CartOperationResult> RemoveItemAsync(
        ClaimsPrincipal user,
        long cartItemId,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return CartOperationResult.Failure("請先登入會員後再調整購物車。");
        }

        var item = await _cartRepository.GetCartItemForUpdateAsync(
            userId,
            cartItemId,
            cancellationToken);

        if (item is null)
        {
            return CartOperationResult.Failure("找不到可刪除的購物車項目。");
        }

        item.Cart.UpdatedAt = DateTime.UtcNow;
        _cartRepository.RemoveCartItem(item);
        await _cartRepository.SaveChangesAsync(cancellationToken);

        return CartOperationResult.Success("已從購物車移除商品。");
    }

    public async Task<CartViewModel> GetCartAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return EmptyCart();
        }

        var cart = await _cartRepository.GetActiveCartForReadAsync(userId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            return EmptyCart();
        }

        var items = BuildCartItems(cart);
        var subtotal = cart.Items.Sum(item => item.UnitPrice * item.Quantity);

        return new CartViewModel
        {
            Items = items,
            AvailableCoupons = BuildCouponOptions(),
            SubtotalText = FormatMoney(subtotal),
            DiscountTotalText = FormatMoney(0m),
            EstimatedTotalText = FormatMoney(subtotal),
            CanCheckout = items.Length > 0
        };
    }

    public async Task<CartCouponPreviewResult> PreviewCouponAsync(
        ClaimsPrincipal user,
        string? couponCode,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(user, out var userId))
        {
            return BuildCouponPreview(false, "請先登入會員後再使用優惠券。", string.Empty, string.Empty, 0m, 0m, false);
        }

        var cart = await _cartRepository.GetActiveCartForReadAsync(userId, cancellationToken);
        if (cart is null || cart.Items.Count == 0)
        {
            return BuildCouponPreview(false, "購物車目前沒有商品，無法試算優惠。", string.Empty, string.Empty, 0m, 0m, false);
        }

        var subtotal = cart.Items.Sum(item => item.UnitPrice * item.Quantity);
        var normalizedCouponCode = NormalizeCouponCode(couponCode);
        if (string.IsNullOrEmpty(normalizedCouponCode))
        {
            return BuildCouponPreview(true, "未套用優惠券。", string.Empty, string.Empty, subtotal, 0m, true);
        }

        var coupon = CouponDefinitions.FirstOrDefault(item =>
            string.Equals(item.CouponCode, normalizedCouponCode, StringComparison.OrdinalIgnoreCase));

        // 目前為前端互動用試算資料；正式優惠券規則需改由 Coupon/Promotion Service 讀資料庫並寫入使用紀錄。
        if (coupon is null)
        {
            return BuildCouponPreview(false, "找不到可用的優惠券。", normalizedCouponCode, string.Empty, subtotal, 0m, true);
        }

        if (subtotal < coupon.MinOrderAmount)
        {
            return BuildCouponPreview(
                false,
                $"此優惠券需滿 {FormatMoney(coupon.MinOrderAmount)} 才可使用。",
                coupon.CouponCode,
                coupon.CouponName,
                subtotal,
                0m,
                true);
        }

        var discountAmount = CalculateDiscountAmount(coupon, subtotal);
        return BuildCouponPreview(
            true,
            $"已套用「{coupon.CouponName}」。",
            coupon.CouponCode,
            coupon.CouponName,
            subtotal,
            discountAmount,
            true);
    }

    private static CartItemViewModel[] BuildCartItems(ShoppingCart cart)
    {
        return cart.Items
            .OrderBy(item => item.CartItemId)
            .Select(item =>
            {
                var lineTotal = item.UnitPrice * item.Quantity;

                return new CartItemViewModel
                {
                    CartItemId = item.CartItemId,
                    ProductId = item.Sku.ProductId,
                    SkuId = item.SkuId,
                    ProductName = item.Sku.Product.ProductName,
                    SkuName = item.Sku.SkuName,
                    Quantity = item.Quantity,
                    UnitPriceText = FormatMoney(item.UnitPrice),
                    LineTotalText = FormatMoney(lineTotal),
                    StockStatusText = item.Sku.Status == "Active" ? "可購買" : "暫停供應"
                };
            })
            .ToArray();
    }

    private static CartViewModel EmptyCart()
    {
        return new CartViewModel
        {
            AvailableCoupons = BuildCouponOptions(),
            SubtotalText = FormatMoney(0m),
            DiscountTotalText = FormatMoney(0m),
            EstimatedTotalText = FormatMoney(0m),
            CanCheckout = false
        };
    }

    private static IReadOnlyList<CartCouponOptionViewModel> BuildCouponOptions()
    {
        return CouponDefinitions
            .Select(coupon => new CartCouponOptionViewModel
            {
                CouponCode = coupon.CouponCode,
                DisplayName = coupon.CouponName,
                Description = coupon.Description
            })
            .ToArray();
    }

    private static CartCouponPreviewResult BuildCouponPreview(
        bool succeeded,
        string message,
        string couponCode,
        string couponName,
        decimal subtotal,
        decimal discountAmount,
        bool canCheckout)
    {
        // 折扣金額在回傳前再次夾限，避免顯示負數或折扣超過小計。
        var normalizedDiscountAmount = Math.Min(Math.Max(discountAmount, 0m), subtotal);
        var estimatedTotal = Math.Max(subtotal - normalizedDiscountAmount, 0m);

        return CartCouponPreviewResult.Create(
            succeeded,
            message,
            couponCode,
            couponName,
            FormatMoney(subtotal),
            FormatMoney(normalizedDiscountAmount),
            FormatMoney(estimatedTotal),
            canCheckout);
    }

    private static decimal CalculateDiscountAmount(CartCouponDefinition coupon, decimal subtotal)
    {
        var discountAmount = coupon.DiscountType switch
        {
            "Amount" => coupon.DiscountValue,
            "Percent" => decimal.Round(subtotal * coupon.DiscountValue / 100m, 0, MidpointRounding.AwayFromZero),
            _ => 0m
        };

        if (coupon.MaxDiscountAmount.HasValue)
        {
            discountAmount = Math.Min(discountAmount, coupon.MaxDiscountAmount.Value);
        }

        return Math.Min(discountAmount, subtotal);
    }

    private static string NormalizeCouponCode(string? couponCode)
    {
        return string.IsNullOrWhiteSpace(couponCode)
            ? string.Empty
            : couponCode.Trim().ToUpperInvariant();
    }

    private static bool TryGetUserId(ClaimsPrincipal user, out long userId)
    {
        // 後端從 Cookie Claim 解析使用者，而不是讓前端提交 UserId。
        if (!IsCustomer(user))
        {
            userId = 0;
            return false;
        }

        var userIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(userIdText, out userId) && userId > 0;
    }

    private static bool IsCustomer(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true
            && user.Claims.Any(claim =>
                (string.Equals(claim.Type, "user_type", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(claim.Type, "UserType", StringComparison.OrdinalIgnoreCase))
                && string.Equals(claim.Value, "Customer", StringComparison.OrdinalIgnoreCase));
    }

    private static string FormatMoney(decimal amount)
    {
        return string.Format(TaiwanCulture, "NT$ {0:N0}", amount);
    }

    private sealed record CartCouponDefinition(
        string CouponCode,
        string CouponName,
        string Description,
        string DiscountType,
        decimal DiscountValue,
        decimal MinOrderAmount,
        decimal? MaxDiscountAmount);
}
