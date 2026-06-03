using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Inventory;

namespace MyWeb.Domain.Entities.Orders;

public class ShoppingCart
{
    public long CartId { get; set; }
    public long UserId { get; set; }
    public string CartStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<ShoppingCartItem> Items { get; set; } = [];
}

public class ShoppingCartItem
{
    public long CartItemId { get; set; }
    public long CartId { get; set; }
    public long SkuId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ShoppingCart Cart { get; set; } = null!;
    public ProductSku Sku { get; set; } = null!;
}

public class Order
{
    public long OrderId { get; set; }
    public string OrderNo { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string ShippingStatus { get; set; } = string.Empty;
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverAddress { get; set; } = string.Empty;
    public string? Remark { get; set; }
    public DateTime OrderedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistories { get; set; } = [];
    public ICollection<InventoryReservation> InventoryReservations { get; set; } = [];
}

public class OrderItem
{
    public long OrderItemId { get; set; }
    public long OrderId { get; set; }
    public long ProductId { get; set; }
    public long SkuId { get; set; }
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public string SkuNameSnapshot { get; set; } = string.Empty;
    public string SkuNoSnapshot { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal SubtotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public ProductSku Sku { get; set; } = null!;
    public ICollection<InventoryReservation> InventoryReservations { get; set; } = [];
}

public class OrderStatusHistory
{
    public long OrderStatusHistoryId { get; set; }
    public long OrderId { get; set; }
    public string? OldStatus { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? ChangedReason { get; set; }
    public DateTime ChangedAt { get; set; }
    public long? ChangedBy { get; set; }

    public Order Order { get; set; } = null!;
    public User? ChangedByUser { get; set; }
}
