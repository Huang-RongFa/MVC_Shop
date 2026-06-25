using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.DTOs.Storefront.Products;

public sealed record ProductCatalogCategoryRecord(
    int CategoryId,
    string CategoryCode,
    string CategoryName,
    string Description,
    int SortOrder);

public sealed record ProductCatalogSearchCriteria(
    string Keyword,
    string CategoryCode,
    string OriginName,
    string Certification,
    int? MinPrice,
    int? MaxPrice,
    string Sort,
    int Page,
    int PageSize,
    bool FeaturedOnly);

public sealed record ProductCatalogSearchResult(
    int TotalCount,
    IReadOnlyList<ProductCatalogItemRecord> Items);

public sealed record ProductCatalogFilterValues(
    IReadOnlyList<string> Origins,
    IReadOnlyList<string> Certifications);

public sealed record ProductCatalogPageResult(
    int TotalCount,
    IReadOnlyList<string> Origins,
    IReadOnlyList<string> Certifications,
    IReadOnlyList<ProductCardViewModel> Products);

public sealed record ProductCatalogItemRecord(
    long ProductId,
    string ProductNo,
    string ProductName,
    string ShortDescription,
    string CategoryCode,
    string CategoryName,
    decimal SalePrice,
    string UnitText,
    DateTime? PublishedAt,
    int AvailableStockQuantity,
    IReadOnlyDictionary<string, string> Attributes);

public sealed record ProductCatalogSkuOptionRecord(
    long SkuId,
    string SkuName,
    string UnitText,
    decimal SalePrice,
    int AvailableStockQuantity,
    string Status);
