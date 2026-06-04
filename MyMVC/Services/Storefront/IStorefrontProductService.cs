using MyWeb.DTOs.Common;
using MyWeb.DTOs.Storefront.Products;

namespace MyWeb.Services.Storefront;

public interface IStorefrontProductService
{
    Task<PagedResult<ProductListItemDto>> SearchProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailDto?> GetProductDetailAsync(
        long productId,
        CancellationToken cancellationToken = default);
}
