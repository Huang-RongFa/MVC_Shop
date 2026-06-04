using MyWeb.DTOs.Common;
using MyWeb.DTOs.Storefront.Products;

namespace MyWeb.Repositories.Catalog;

public interface IProductRepository
{
    Task<PagedResult<ProductListItemDto>> SearchStorefrontProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailDto?> GetStorefrontProductDetailAsync(
        long productId,
        CancellationToken cancellationToken = default);
}
