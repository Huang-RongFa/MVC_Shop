using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Orders;

namespace MyWeb.Domain.Entities.Promotions;

public class Coupon
{
    public long CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string CouponName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public int? TotalUsageLimit { get; set; }
    public int? PerUserUsageLimit { get; set; }
    public int UsedCount { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<CouponUsage> Usages { get; set; } = [];
    public ICollection<OrderDiscount> OrderDiscounts { get; set; } = [];
}

public class CouponUsage
{
    public long CouponUsageId { get; set; }
    public long CouponId { get; set; }
    public long UserId { get; set; }
    public long OrderId { get; set; }
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; }

    public Coupon Coupon { get; set; } = null!;
    public User User { get; set; } = null!;
    public Order Order { get; set; } = null!;
}

public class Promotion
{
    public long PromotionId { get; set; }
    public string PromotionCode { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string PromotionType { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<PromotionProduct> Products { get; set; } = [];
    public ICollection<OrderDiscount> OrderDiscounts { get; set; } = [];
}

public class PromotionProduct
{
    public long PromotionProductId { get; set; }
    public long PromotionId { get; set; }
    public long? ProductId { get; set; }
    public long? SkuId { get; set; }

    public Promotion Promotion { get; set; } = null!;
    public Product? Product { get; set; }
    public ProductSku? Sku { get; set; }
}

public class OrderDiscount
{
    public long OrderDiscountId { get; set; }
    public long OrderId { get; set; }
    public string DiscountSourceType { get; set; } = string.Empty;
    public long? CouponId { get; set; }
    public long? PromotionId { get; set; }
    public string DiscountName { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Coupon? Coupon { get; set; }
    public Promotion? Promotion { get; set; }
}
