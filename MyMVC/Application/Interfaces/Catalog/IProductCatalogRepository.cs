using MyWeb.Application.DTOs.Storefront.Products;

namespace MyWeb.Application.Interfaces.Catalog;

public interface IProductCatalogRepository
{
    Task<IReadOnlyList<ProductCatalogCategoryRecord>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken);

    Task<ProductCatalogSearchResult> SearchActiveProductsAsync(
        ProductCatalogSearchCriteria criteria,
        CancellationToken cancellationToken);

    Task<ProductCatalogFilterValues> GetFilterValuesAsync(
        CancellationToken cancellationToken);

    Task<ProductCatalogItemRecord?> GetActiveProductAsync(
        long productId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductCatalogSkuOptionRecord>> GetActiveSkuOptionsAsync(
        long productId,
        CancellationToken cancellationToken);
}
