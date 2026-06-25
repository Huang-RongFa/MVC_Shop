namespace MyWeb.Application.DTOs.Admin.Commerce;

public sealed record AdminProductListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyCollection<AdminProductListItemDto> Products,
    IReadOnlyCollection<string> AvailableStatuses,
    IReadOnlyCollection<AdminCategoryOptionDto> CategoryOptions);

public sealed record AdminProductListItemDto(
    long ProductId,
    string ProductNo,
    string ProductName,
    string? BrandName,
    string CategoryName,
    string Status,
    int SkuCount,
    int ActiveSkuCount,
    decimal? MinSalePrice,
    decimal? MaxSalePrice,
    string? MainImageUrl,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AdminProductEditResponse(
    long? ProductId,
    string? ProductNo,
    int? CategoryId,
    string ProductName,
    string? BrandName,
    string? ShortDescription,
    string? FullDescription,
    string Status,
    bool IsFeatured,
    AdminProductSkuEditDto Sku,
    IReadOnlyCollection<AdminProductImageEditDto> Images,
    IReadOnlyCollection<string> AvailableStatuses,
    IReadOnlyCollection<string> AvailableSkuStatuses,
    IReadOnlyCollection<AdminCategoryOptionDto> CategoryOptions);

public sealed record AdminProductSkuEditDto(
    long? SkuId,
    string? SkuNo,
    string SkuName,
    string? SpecText,
    decimal ListPrice,
    decimal SalePrice,
    decimal? BasePrice,
    string? Size,
    string? Color,
    string? Capacity,
    string Status);

public sealed record AdminProductImageEditDto(
    long? ImageId,
    string ImageUrl,
    string? AltText,
    bool IsMainImage,
    int SortOrder);

public sealed record AdminProductUpsertRequest(
    int CategoryId,
    string ProductName,
    string? BrandName,
    string? ShortDescription,
    string? FullDescription,
    string Status,
    bool IsFeatured,
    AdminProductSkuUpsertRequest Sku,
    string? MainImageUrl,
    IReadOnlyCollection<AdminProductImageUpsertRequest>? Images,
    string? Reason);

public sealed record AdminProductSkuUpsertRequest(
    long? SkuId,
    string? SkuNo,
    string SkuName,
    string? SpecText,
    decimal ListPrice,
    decimal SalePrice,
    decimal? BasePrice,
    string? Size,
    string? Color,
    string? Capacity,
    string Status);

public sealed record AdminProductImageUpsertRequest(
    long? ImageId,
    string ImageUrl,
    string? AltText,
    bool IsMainImage,
    int SortOrder);

public sealed record AdminProductMutationResponse(
    string Message,
    long ProductId);

public sealed record AdminCategoryOptionDto(
    int CategoryId,
    string CategoryCode,
    string CategoryName,
    bool IsActive);

public sealed record AdminSkuListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyCollection<AdminSkuListItemDto> Skus,
    IReadOnlyCollection<string> AvailableStatuses);

public sealed record AdminSkuListItemDto(
    long SkuId,
    string SkuNo,
    string ProductNo,
    string ProductName,
    string SkuName,
    string? SpecText,
    decimal ListPrice,
    decimal SalePrice,
    string Status,
    int TotalOnHandQty,
    int TotalReservedQty,
    int TotalAvailableQty,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record AdminInventoryListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyCollection<AdminInventoryListItemDto> Stocks,
    IReadOnlyCollection<AdminWarehouseOptionDto> WarehouseOptions);

public sealed record AdminInventoryListItemDto(
    long InventoryStockId,
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    long SkuId,
    string SkuNo,
    string ProductName,
    string SkuName,
    int OnHandQty,
    int ReservedQty,
    int AvailableQty,
    int SafetyStockQty,
    string StockStatus,
    DateTime? UpdatedAt);

public sealed record AdminWarehouseOptionDto(
    int WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    bool IsActive);

public sealed record AdminOrderListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyCollection<AdminOrderListItemDto> Orders,
    IReadOnlyCollection<string> OrderStatuses,
    IReadOnlyCollection<string> PaymentStatuses,
    IReadOnlyCollection<string> ShippingStatuses);

public sealed record AdminOrderListItemDto(
    long OrderId,
    string OrderNo,
    long UserId,
    string CustomerName,
    string OrderStatus,
    string PaymentStatus,
    string ShippingStatus,
    decimal TotalAmount,
    int ItemCount,
    DateTime OrderedAt,
    DateTime? PaidAt);
