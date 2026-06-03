using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Inventory;
using MyWeb.Domain.Entities.Orders;

namespace MyWeb.Domain.Entities.Payments;

public class Payment
{
    public long PaymentId { get; set; }
    public string PaymentNo { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public ICollection<PaymentTransaction> Transactions { get; set; } = [];
}

public class PaymentTransaction
{
    public long PaymentTransactionId { get; set; }
    public long PaymentId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderTransactionNo { get; set; }
    public string TransactionStatus { get; set; } = string.Empty;
    public string? RequestPayload { get; set; }
    public string? ResponsePayload { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }

    public Payment Payment { get; set; } = null!;
}

public class Shipment
{
    public long ShipmentId { get; set; }
    public string ShipmentNo { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public int? WarehouseId { get; set; }
    public string? CarrierName { get; set; }
    public string? TrackingNo { get; set; }
    public string ShipmentStatus { get; set; } = string.Empty;
    public DateTime? ShippedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Warehouse? Warehouse { get; set; }
    public ICollection<ShipmentItem> Items { get; set; } = [];
}

public class ShipmentItem
{
    public long ShipmentItemId { get; set; }
    public long ShipmentId { get; set; }
    public long OrderItemId { get; set; }
    public int Quantity { get; set; }

    public Shipment Shipment { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
}

public class Refund
{
    public long RefundId { get; set; }
    public string RefundNo { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public long? PaymentId { get; set; }
    public string RefundStatus { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public string? Reason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public long? CreatedBy { get; set; }

    public Order Order { get; set; } = null!;
    public Payment? Payment { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<RefundItem> Items { get; set; } = [];
}

public class RefundItem
{
    public long RefundItemId { get; set; }
    public long RefundId { get; set; }
    public long OrderItemId { get; set; }
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }

    public Refund Refund { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
}
