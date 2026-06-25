using Microsoft.EntityFrameworkCore;
using MyWeb.Application.DTOs.Storefront.Products;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Data;
using MyWeb.Domain.Entities.Catalog;

namespace MyWeb.Infrastructure.Repositories.Catalog;

/// <summary>
/// 前台商品目錄資料存取。
/// 只回傳已上架、未刪除且具備可販售 SKU 的商品投影，不直接暴露 Entity 給 Controller 或 View。
/// </summary>
public sealed class ProductCatalogRepository : IProductCatalogRepository
{
    private const string ActiveProductStatus = "Active";
    private const string ActiveSkuStatus = "Active";
    private const string OriginAttributeCode = "ORIGIN";
    private const string CertificationAttributeCode = "CERTIFICATION";

    private readonly AppDbContext _dbContext;

    public ProductCatalogRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ProductCatalogCategoryRecord>> GetActiveCategoriesAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductCategories
            .AsNoTracking()
            .Where(category => category.IsActive && !category.IsDeleted)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.CategoryId)
            .Select(category => new ProductCatalogCategoryRecord(
                category.CategoryId,
                category.CategoryCode,
                category.CategoryName,
                category.Description ?? string.Empty,
                category.SortOrder))
            .ToArrayAsync(cancellationToken);
    }

    public async Task<ProductCatalogSearchResult> SearchActiveProductsAsync(
        ProductCatalogSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        var page = criteria.Page < 1 ? 1 : criteria.Page;
        var pageSize = Math.Clamp(criteria.PageSize, 1, 48);
        var query = BuildActiveProductQuery();

        if (criteria.FeaturedOnly)
        {
            query = query.Where(product => product.IsFeatured);
        }

        if (!string.IsNullOrWhiteSpace(criteria.Keyword))
        {
            var keyword = criteria.Keyword.Trim();
            query = query.Where(product =>
                product.ProductName.Contains(keyword)
                || (product.ShortDescription != null && product.ShortDescription.Contains(keyword))
                || product.Category.CategoryName.Contains(keyword)
                || _dbContext.ProductAttributeValues.Any(value =>
                    value.Sku.ProductId == product.ProductId
                    && value.AttributeValue.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(criteria.CategoryCode))
        {
            var categoryCode = criteria.CategoryCode.Trim();
            query = query.Where(product => product.Category.CategoryCode == categoryCode);
        }

        if (!string.IsNullOrWhiteSpace(criteria.OriginName))
        {
            var originName = criteria.OriginName.Trim();
            query = query.Where(product => _dbContext.ProductAttributeValues.Any(value =>
                value.Sku.ProductId == product.ProductId
                && value.Attribute.AttributeCode == OriginAttributeCode
                && value.AttributeValue == originName));
        }

        if (!string.IsNullOrWhiteSpace(criteria.Certification))
        {
            var certification = criteria.Certification.Trim();
            query = query.Where(product => _dbContext.ProductAttributeValues.Any(value =>
                value.Sku.ProductId == product.ProductId
                && value.Attribute.AttributeCode == CertificationAttributeCode
                && value.AttributeValue.Contains(certification)));
        }

        if (criteria.MinPrice is not null)
        {
            query = query.Where(product =>
                product.Skus
                    .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                    .Min(sku => sku.SalePrice) >= criteria.MinPrice.Value);
        }

        if (criteria.MaxPrice is not null)
        {
            query = query.Where(product =>
                product.Skus
                    .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                    .Min(sku => sku.SalePrice) <= criteria.MaxPrice.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var productRows = await ProjectProducts(ApplySort(query, criteria.Sort)
                .Skip((page - 1) * pageSize)
                .Take(pageSize))
            .ToArrayAsync(cancellationToken);
        var productIds = productRows.Select(product => product.ProductId).ToArray();
        // 屬性值另批次查詢後依 ProductId 合併，避免在每張商品卡產生額外查詢。
        var attributesByProductId = await GetAttributesByProductIdsAsync(productIds, cancellationToken);

        return new ProductCatalogSearchResult(
            totalCount,
            productRows
                .Select(product => product with
                {
                    Attributes = attributesByProductId.GetValueOrDefault(
                        product.ProductId,
                        new Dictionary<string, string>())
                })
                .ToArray());
    }

    public async Task<ProductCatalogFilterValues> GetFilterValuesAsync(
        CancellationToken cancellationToken)
    {
        var rows = await _dbContext.ProductAttributeValues
            .AsNoTracking()
            .Where(value =>
                (value.Attribute.AttributeCode == OriginAttributeCode
                    || value.Attribute.AttributeCode == CertificationAttributeCode)
                && value.Sku.Status == ActiveSkuStatus
                && !value.Sku.IsDeleted
                && value.Sku.Product.Status == ActiveProductStatus
                && !value.Sku.Product.IsDeleted
                && value.Sku.Product.Category.IsActive
                && !value.Sku.Product.Category.IsDeleted)
            .Select(value => new
            {
                value.Attribute.AttributeCode,
                value.AttributeValue
            })
            .ToArrayAsync(cancellationToken);

        var origins = rows
            .Where(row => row.AttributeCode == OriginAttributeCode)
            .Select(row => row.AttributeValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToArray();
        var certifications = rows
            .Where(row => row.AttributeCode == CertificationAttributeCode)
            .SelectMany(row => SplitCsv(row.AttributeValue))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToArray();

        return new ProductCatalogFilterValues(origins, certifications);
    }

    public async Task<ProductCatalogItemRecord?> GetActiveProductAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        var product = await ProjectProducts(
                BuildActiveProductQuery().Where(product => product.ProductId == productId))
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            return null;
        }

        var attributesByProductId = await GetAttributesByProductIdsAsync(
            [product.ProductId],
            cancellationToken);

        return product with
        {
            Attributes = attributesByProductId.GetValueOrDefault(
                product.ProductId,
                new Dictionary<string, string>())
        };
    }

    public async Task<IReadOnlyList<ProductCatalogSkuOptionRecord>> GetActiveSkuOptionsAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProductSkus
            .AsNoTracking()
            .Where(sku =>
                sku.ProductId == productId
                && sku.Status == ActiveSkuStatus
                && !sku.IsDeleted
                && sku.Product.Status == ActiveProductStatus
                && !sku.Product.IsDeleted)
            .OrderBy(sku => sku.SalePrice)
            .ThenBy(sku => sku.SkuId)
            .Select(sku => new ProductCatalogSkuOptionRecord(
                sku.SkuId,
                sku.SkuName,
                sku.SpecText ?? string.Empty,
                sku.SalePrice,
                _dbContext.InventoryStocks
                    .Where(stock => stock.SkuId == sku.SkuId)
                    .Sum(stock => (int?)stock.AvailableQty) ?? 0,
                sku.Status))
            .ToArrayAsync(cancellationToken);
    }

    private IQueryable<Product> BuildActiveProductQuery()
    {
        // 前台商品查詢的共同守門條件集中在這裡，避免列表與詳細頁出現不同上架規則。
        return _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                product.Status == ActiveProductStatus
                && !product.IsDeleted
                && product.Category.IsActive
                && !product.Category.IsDeleted
                && product.Skus.Any(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted));
    }

    private IQueryable<Product> ApplySort(IQueryable<Product> query, string sort)
    {
        // 只接受 Service 正規化後的排序代碼，不允許前端直接指定任意欄位名稱。
        return sort switch
        {
            "price-asc" => query
                .OrderBy(product => product.Skus
                    .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                    .Min(sku => sku.SalePrice))
                .ThenBy(product => product.ProductId),
            "price-desc" => query
                .OrderByDescending(product => product.Skus
                    .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                    .Min(sku => sku.SalePrice))
                .ThenBy(product => product.ProductId),
            "new" => query
                .OrderByDescending(product => product.PublishedAt ?? product.CreatedAt)
                .ThenBy(product => product.ProductId),
            "rating" => query
                .OrderByDescending(product => product.IsFeatured)
                .ThenByDescending(product => product.PublishedAt ?? product.CreatedAt)
                .ThenBy(product => product.ProductId),
            _ => query
                .OrderByDescending(product => product.IsFeatured)
                .ThenBy(product => product.ProductId)
        };
    }

    private IQueryable<ProductCatalogItemRecord> ProjectProducts(IQueryable<Product> query)
    {
        // 以投影查詢取得前台需要的欄位，避免 Include 整個 Entity Graph 後再於記憶體裁切。
        return query.Select(product => new ProductCatalogItemRecord(
            product.ProductId,
            product.ProductNo,
            product.ProductName,
            product.ShortDescription ?? string.Empty,
            product.Category.CategoryCode,
            product.Category.CategoryName,
            product.Skus
                .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                .OrderBy(sku => sku.SalePrice)
                .Select(sku => sku.SalePrice)
                .FirstOrDefault(),
            product.Skus
                .Where(sku => sku.Status == ActiveSkuStatus && !sku.IsDeleted)
                .OrderBy(sku => sku.SalePrice)
                .Select(sku => sku.SpecText ?? string.Empty)
                .FirstOrDefault() ?? string.Empty,
            product.PublishedAt,
            _dbContext.InventoryStocks
                .Where(stock =>
                    stock.Sku.ProductId == product.ProductId
                    && stock.Sku.Status == ActiveSkuStatus
                    && !stock.Sku.IsDeleted)
                .Sum(stock => (int?)stock.AvailableQty) ?? 0,
            new Dictionary<string, string>()));
    }

    private async Task<IReadOnlyDictionary<long, IReadOnlyDictionary<string, string>>> GetAttributesByProductIdsAsync(
        IReadOnlyCollection<long> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<long, IReadOnlyDictionary<string, string>>();
        }

        var rows = await _dbContext.ProductAttributeValues
            .AsNoTracking()
            .Where(value =>
                productIds.Contains(value.Sku.ProductId)
                && value.Sku.Status == ActiveSkuStatus
                && !value.Sku.IsDeleted)
            .Select(value => new
            {
                value.Sku.ProductId,
                value.Attribute.AttributeCode,
                value.AttributeValue
            })
            .ToArrayAsync(cancellationToken);

        // 同一商品同一屬性可能來自多個 SKU；前台卡片只取第一個可顯示值，詳細 SKU 仍由 SKU 查詢提供。
        return rows
            .GroupBy(row => row.ProductId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyDictionary<string, string>)group
                    .GroupBy(row => row.AttributeCode)
                    .ToDictionary(
                        attributeGroup => attributeGroup.Key,
                        attributeGroup => attributeGroup
                            .Select(row => row.AttributeValue)
                            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty));
    }

    private static IEnumerable<string> SplitCsv(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item));
    }
}
