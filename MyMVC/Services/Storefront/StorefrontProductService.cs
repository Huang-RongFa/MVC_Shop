using MyWeb.DTOs.Common;
using MyWeb.DTOs.Storefront.Products;
using MyWeb.Repositories.Catalog;

namespace MyWeb.Services.Storefront;

public class StorefrontProductService : IStorefrontProductService
{
    private readonly IProductRepository _productRepository;

    public StorefrontProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<PagedResult<ProductListItemDto>> SearchProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        return _productRepository.SearchStorefrontProductsAsync(query, cancellationToken);
    }

    public Task<ProductDetailDto?> GetProductDetailAsync(
        long productId,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return Task.FromResult<ProductDetailDto?>(null);
        }

        return _productRepository.GetStorefrontProductDetailAsync(productId, cancellationToken);
    }
}
