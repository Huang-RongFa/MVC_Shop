namespace MyWeb.Models.ViewModels.Storefront;

public sealed class StorefrontHomeViewModel
{
    public bool IsAuthenticated { get; init; }

    public bool CanShopAsCustomer { get; init; }

    public int CartItemCount { get; init; }

    public IReadOnlyList<CategoryCardViewModel> Categories { get; init; } = [];

    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; init; } = [];

    public IReadOnlyList<PromotionBannerViewModel> PromotionBanners { get; init; } = [];
}

public sealed class CategoryCardViewModel
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string VisualClass { get; init; } = string.Empty;
}

public sealed class ProductCardViewModel
{
    public long ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string ShortDescription { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string OriginName { get; init; } = string.Empty;

    public string FarmerName { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public decimal PriceAmount { get; init; }

    public string UnitText { get; init; } = string.Empty;

    public string PromotionLabel { get; init; } = string.Empty;

    public string StockStatusText { get; init; } = string.Empty;

    public string StockStatusVariant { get; init; } = "success";

    public int AvailableStockQuantity { get; init; }

    public int CurrentCartQuantity { get; init; }

    public string VisualClass { get; init; } = string.Empty;

    public string RatingText { get; init; } = string.Empty;

    public DateTime PublishedAt { get; init; }

    public IReadOnlyList<string> Certifications { get; init; } = [];

    public bool CanAddToCart { get; init; }
}

public sealed class AddToCartModalViewModel
{
    public long ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public string VisualClass { get; init; } = string.Empty;

    public int AvailableStockQuantity { get; init; }

    public int CurrentCartQuantity { get; init; }

    public string ReturnUrl { get; init; } = string.Empty;
}

public sealed class PromotionBannerViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string BadgeText { get; init; } = string.Empty;
}

public sealed class ProductListViewModel
{
    public bool IsAuthenticated { get; init; }

    public bool CanShopAsCustomer { get; init; }

    public string ListTitle { get; init; } = "商品列表";

    public string Keyword { get; init; } = string.Empty;

    public string SelectedCategorySlug { get; init; } = string.Empty;

    public string SelectedOrigin { get; init; } = string.Empty;

    public string SelectedCertification { get; init; } = string.Empty;

    public string Sort { get; init; } = "popular";

    public string ViewMode { get; init; } = "grid";

    public int? MinPrice { get; init; }

    public int? MaxPrice { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 12;

    public int TotalCount { get; init; }

    public IReadOnlyList<CategoryCardViewModel> Categories { get; init; } = [];

    public IReadOnlyList<string> Origins { get; init; } = [];

    public IReadOnlyList<string> Certifications { get; init; } = [];

    public IReadOnlyList<ProductCardViewModel> Products { get; init; } = [];
}

public sealed class ProductDetailViewModel
{
    public long ProductId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string OriginName { get; init; } = string.Empty;

    public string FarmerName { get; init; } = string.Empty;

    public string PriceRange { get; init; } = string.Empty;

    public string UnitText { get; init; } = string.Empty;

    public string StockStatusText { get; init; } = string.Empty;

    public string StockStatusVariant { get; init; } = "success";

    public int AvailableStockQuantity { get; init; }

    public int CurrentCartQuantity { get; init; }

    public string VisualClass { get; init; } = string.Empty;

    public bool IsAuthenticated { get; init; }

    public bool CanShopAsCustomer { get; init; }

    public bool CanAddToCart { get; init; }

    public IReadOnlyList<ProductSkuOptionViewModel> Skus { get; init; } = [];

    public IReadOnlyList<string> Highlights { get; init; } = [];

    public IReadOnlyList<string> Certifications { get; init; } = [];
}

public sealed class ProductSkuOptionViewModel
{
    public long SkuId { get; init; }

    public string SkuName { get; init; } = string.Empty;

    public string PriceText { get; init; } = string.Empty;

    public string StockStatusText { get; init; } = string.Empty;
}

public sealed class CartViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; init; } = [];

    public IReadOnlyList<CartCouponOptionViewModel> AvailableCoupons { get; init; } = [];

    public string SelectedCouponCode { get; init; } = string.Empty;

    public string SubtotalText { get; init; } = string.Empty;

    public string DiscountTotalText { get; init; } = string.Empty;

    public string EstimatedTotalText { get; init; } = string.Empty;

    public bool CanCheckout { get; init; }
}

public sealed class CartCouponOptionViewModel
{
    public string CouponCode { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}

public sealed class CartItemViewModel
{
    public long CartItemId { get; init; }

    public long ProductId { get; init; }

    public long SkuId { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public string SkuName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public string UnitPriceText { get; init; } = string.Empty;

    public string LineTotalText { get; init; } = string.Empty;

    public string StockStatusText { get; init; } = string.Empty;
}

public sealed class CheckoutViewModel
{
    public IReadOnlyList<CartItemViewModel> Items { get; init; } = [];

    public ShippingInfoViewModel ShippingInfo { get; init; } = new();

    public IReadOnlyList<PaymentMethodViewModel> PaymentMethods { get; init; } = [];

    public OrderSummaryViewModel OrderSummary { get; init; } = new();
}

public sealed class ShippingInfoViewModel
{
    public string RecipientName { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;
}

public sealed class PaymentMethodViewModel
{
    public string Code { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}

public sealed class OrderSummaryViewModel
{
    public string SubtotalText { get; init; } = string.Empty;

    public string DiscountTotalText { get; init; } = string.Empty;

    public string ShippingFeeText { get; init; } = string.Empty;

    public string PayableTotalText { get; init; } = string.Empty;
}

public sealed class CheckoutCompleteViewModel
{
    public string OrderNumber { get; init; } = string.Empty;

    public string OrderStatusText { get; init; } = string.Empty;

    public string PaymentStatusText { get; init; } = string.Empty;

    public string NextStepText { get; init; } = string.Empty;
}

public sealed class OrderListViewModel
{
    public IReadOnlyList<OrderSummaryRowViewModel> Orders { get; init; } = [];
}

public sealed class OrderSummaryRowViewModel
{
    public int OrderId { get; init; }

    public string OrderNumber { get; init; } = string.Empty;

    public string CreatedAtText { get; init; } = string.Empty;

    public string TotalText { get; init; } = string.Empty;

    public string OrderStatusText { get; init; } = string.Empty;

    public string PaymentStatusText { get; init; } = string.Empty;

    public string ShipmentStatusText { get; init; } = string.Empty;
}

public sealed class OrderDetailViewModel
{
    public string OrderNumber { get; init; } = string.Empty;

    public string OrderStatusText { get; init; } = string.Empty;

    public string PaymentStatusText { get; init; } = string.Empty;

    public string ShipmentStatusText { get; init; } = string.Empty;

    public string TotalText { get; init; } = string.Empty;

    public IReadOnlyList<OrderLineViewModel> Items { get; init; } = [];

    public IReadOnlyList<TimelineItemViewModel> Timeline { get; init; } = [];
}

public sealed class OrderLineViewModel
{
    public string ProductName { get; init; } = string.Empty;

    public string SkuName { get; init; } = string.Empty;

    public int Quantity { get; init; }

    public string UnitPriceText { get; init; } = string.Empty;

    public string LineTotalText { get; init; } = string.Empty;
}

public sealed class TimelineItemViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string OccurredAtText { get; init; } = string.Empty;
}

public sealed class FarmerStoryPageViewModel
{
    public string SelectedFilter { get; init; } = "全部";

    public IReadOnlyList<FarmerFilterViewModel> Filters { get; init; } = [];

    public IReadOnlyList<FarmerStoryCardViewModel> Farmers { get; init; } = [];

    public IReadOnlyList<FarmerRegionViewModel> Regions { get; init; } = [];

    public IReadOnlyList<FarmerPromiseViewModel> Promises { get; init; } = [];
}

public sealed class FarmerFilterViewModel
{
    public string Label { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;
}

public sealed class FarmerStoryCardViewModel
{
    public int FarmerId { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Avatar { get; init; } = string.Empty;

    public string Region { get; init; } = string.Empty;

    public string RegionTag { get; init; } = string.Empty;

    public string VisualClass { get; init; } = string.Empty;

    public string ProduceEmoji { get; init; } = string.Empty;

    public int YearsOfExperience { get; init; }

    public string Story { get; init; } = string.Empty;

    public string OrderCountText { get; init; } = string.Empty;

    public string RatingText { get; init; } = string.Empty;

    public IReadOnlyList<string> ProduceTags { get; init; } = [];

    public IReadOnlyList<string> Certifications { get; init; } = [];
}

public sealed class FarmerRegionViewModel
{
    public string Emoji { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string CountText { get; init; } = string.Empty;
}

public sealed class FarmerPromiseViewModel
{
    public string Icon { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}
