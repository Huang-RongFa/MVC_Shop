using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.DTOs.Common;
using MyWeb.DTOs.Storefront.Products;
using MyWeb.Repositories.Catalog;

namespace MyWeb.Infrastructure.Repositories.Catalog;

public class ProductRepository : IProductRepository
{
    private const string ActiveStatus = "Active";

    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ProductListItemDto>> SearchStorefrontProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = BuildStorefrontProductQuery(query);
        var totalCount = await products.CountAsync(cancellationToken);

        products = ApplySorting(products, query.NormalizedSortBy);

        var items = await products
            .Skip(query.Skip)
            .Take(query.NormalizedPageSize)
            .Select(product => new ProductListItemDto
            {
                ProductId = product.ProductId,
                ProductNo = product.ProductNo,
                ProductName = product.ProductName,
                BrandName = product.BrandName,
                ShortDescription = product.ShortDescription,
                CategoryName = product.Category.CategoryName,
                MainImageUrl = product.Images
                    .OrderByDescending(image => image.IsMainImage)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault(),
                MinSalePrice = product.Skus
                    .Where(sku => !sku.IsDeleted && sku.Status == ActiveStatus)
                    .Min(sku => (decimal?)sku.SalePrice) ?? 0m,
                IsFeatured = product.IsFeatured
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(
            items,
            totalCount,
            query.NormalizedPage,
            query.NormalizedPageSize);
    }

    public Task<ProductDetailDto?> GetStorefrontProductDetailAsync(
        long productId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.ProductId == productId &&
                !product.IsDeleted &&
                product.Status == ActiveStatus &&
                product.Category.IsActive &&
                !product.Category.IsDeleted)
            .Select(product => new ProductDetailDto
            {
                ProductId = product.ProductId,
                ProductNo = product.ProductNo,
                ProductName = product.ProductName,
                BrandName = product.BrandName,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                CategoryName = product.Category.CategoryName,
                Images = product.Images
                    .OrderByDescending(image => image.IsMainImage)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => new ProductImageDto
                    {
                        ImageUrl = image.ImageUrl,
                        AltText = image.AltText,
                        IsMainImage = image.IsMainImage,
                        SortOrder = image.SortOrder
                    })
                    .ToList(),
                Skus = product.Skus
                    .Where(sku => !sku.IsDeleted && sku.Status == ActiveStatus)
                    .OrderBy(sku => sku.SkuId)
                    .Select(sku => new ProductSkuOptionDto
                    {
                        SkuId = sku.SkuId,
                        SkuNo = sku.SkuNo,
                        SkuName = sku.SkuName,
                        SpecText = sku.SpecText,
                        ListPrice = sku.ListPrice,
                        SalePrice = sku.SalePrice
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<Product> BuildStorefrontProductQuery(ProductListQuery query)
    {
        var products = _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                !product.IsDeleted &&
                product.Status == ActiveStatus &&
                product.Category.IsActive &&
                !product.Category.IsDeleted &&
                product.Skus.Any(sku => !sku.IsDeleted && sku.Status == ActiveStatus));

        if (query.CategoryId.HasValue)
        {
            products = products.Where(product => product.CategoryId == query.CategoryId.Value);
        }

        if (query.NormalizedKeyword is { Length: > 0 } keyword)
        {
            products = products.Where(product =>
                product.ProductName.Contains(keyword) ||
                (product.BrandName != null && product.BrandName.Contains(keyword)) ||
                (product.ShortDescription != null && product.ShortDescription.Contains(keyword)) ||
                product.Skus.Any(sku =>
                    !sku.IsDeleted &&
                    sku.Status == ActiveStatus &&
                    (sku.SkuName.Contains(keyword) || sku.SkuNo.Contains(keyword))));
        }

        return products;
    }

    private static IQueryable<Product> ApplySorting(
        IQueryable<Product> products,
        string sortBy)
    {
        return sortBy switch
        {
            "newest" => products
                .OrderByDescending(product => product.PublishedAt ?? product.CreatedAt)
                .ThenByDescending(product => product.ProductId),
            "price_asc" => products
                .OrderBy(product => product.Skus
                    .Where(sku => !sku.IsDeleted && sku.Status == ActiveStatus)
                    .Min(sku => (decimal?)sku.SalePrice) ?? 0m)
                .ThenBy(product => product.ProductId),
            "price_desc" => products
                .OrderByDescending(product => product.Skus
                    .Where(sku => !sku.IsDeleted && sku.Status == ActiveStatus)
                    .Min(sku => (decimal?)sku.SalePrice) ?? 0m)
                .ThenByDescending(product => product.ProductId),
            _ => products
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.PublishedAt ?? product.CreatedAt)
                .ThenByDescending(product => product.ProductId)
        };
    }
}
