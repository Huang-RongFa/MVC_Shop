using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Orders;

namespace MyWeb.Domain.Entities.Inventory;

public class Warehouse
{
    public int WarehouseId { get; set; }
    public string WarehouseCode { get; set; } = string.Empty;
    public string WarehouseName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<InventoryStock> Stocks { get; set; } = [];
    public ICollection<InventoryTransaction> Transactions { get; set; } = [];
}

public class InventoryStock
{
    public long InventoryStockId { get; set; }
    public int WarehouseId { get; set; }
    public long SkuId { get; set; }
    public int OnHandQty { get; set; }
    public int ReservedQty { get; set; }
    public int AvailableQty { get; private set; }
    public int SafetyStockQty { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ProductSku Sku { get; set; } = null!;
}

public class InventoryTransaction
{
    public long InventoryTransactionId { get; set; }
    public string TransactionNo { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public long SkuId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int BeforeQty { get; set; }
    public int AfterQty { get; set; }
    public string? ReferenceType { get; set; }
    public long? ReferenceId { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedAt { get; set; }
    public long? CreatedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ProductSku Sku { get; set; } = null!;
    public User? CreatedByUser { get; set; }
}

public class InventoryReservation
{
    public long ReservationId { get; set; }
    public long OrderId { get; set; }
    public long OrderItemId { get; set; }
    public int WarehouseId { get; set; }
    public long SkuId { get; set; }
    public int ReservedQty { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }

    public Order Order { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
    public Warehouse Warehouse { get; set; } = null!;
    public ProductSku Sku { get; set; } = null!;
}
