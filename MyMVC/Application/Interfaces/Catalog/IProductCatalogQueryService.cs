using MyWeb.Application.DTOs.Storefront.Products;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Interfaces.Catalog;

public interface IProductCatalogQueryService
{
    Task<IReadOnlyList<CategoryCardViewModel>> GetCategoriesAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductCardViewModel>> GetFeaturedProductsAsync(
        int take,
        CancellationToken cancellationToken);

    Task<ProductCatalogPageResult> SearchProductsAsync(
        ProductCatalogSearchCriteria criteria,
        CancellationToken cancellationToken);

    Task<ProductDetailViewModel?> GetProductDetailAsync(
        long productId,
        CancellationToken cancellationToken);
}
