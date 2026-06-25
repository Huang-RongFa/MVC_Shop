using Microsoft.EntityFrameworkCore;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Inventory;

namespace MyWeb.Data.Seed;

public sealed class StorefrontCatalogSeeder
{
    private const string WarehouseCode = "ONLINE";
    private readonly AppDbContext _dbContext;
    private readonly ILogger<StorefrontCatalogSeeder> _logger;

    public StorefrontCatalogSeeder(
        AppDbContext dbContext,
        ILogger<StorefrontCatalogSeeder> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;

            await SeedCategoriesAsync(now, cancellationToken);
            await SeedAttributesAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var categoryCodes = StorefrontCatalogSeedData.Categories.Select(seed => seed.Code).ToArray();
            var attributeCodes = StorefrontCatalogSeedData.Attributes.Select(seed => seed.Code).ToArray();
            var categoriesByCode = await _dbContext.ProductCategories
                .Where(category => categoryCodes.Contains(category.CategoryCode))
                .ToDictionaryAsync(category => category.CategoryCode, cancellationToken);
            var attributesByCode = await _dbContext.ProductAttributes
                .Where(attribute => attributeCodes.Contains(attribute.AttributeCode))
                .ToDictionaryAsync(attribute => attribute.AttributeCode, cancellationToken);
            var warehouse = await EnsureWarehouseAsync(cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            foreach (var seed in StorefrontCatalogSeedData.Products)
            {
                if (!categoriesByCode.TryGetValue(seed.CategoryCode, out var category))
                {
                    throw new InvalidOperationException($"Seed category '{seed.CategoryCode}' was not created.");
                }

                var product = await EnsureProductAsync(seed, category.CategoryId, now, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var sku = await EnsureSkuAsync(seed, product.ProductId, now, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await EnsureStockAsync(seed, warehouse.WarehouseId, sku.SkuId, now, cancellationToken);
                await EnsureProductAttributesAsync(seed, sku.SkuId, attributesByCode, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
        });
        _logger.LogInformation("Storefront catalog seed completed. Products: {ProductCount}", StorefrontCatalogSeedData.Products.Count);
    }

    private async Task SeedCategoriesAsync(DateTime now, CancellationToken cancellationToken)
    {
        var categoryCodes = StorefrontCatalogSeedData.Categories.Select(seed => seed.Code).ToArray();
        var existingCategories = await _dbContext.ProductCategories
            .Where(category => categoryCodes.Contains(category.CategoryCode))
            .ToDictionaryAsync(category => category.CategoryCode, cancellationToken);

        foreach (var seed in StorefrontCatalogSeedData.Categories)
        {
            if (existingCategories.TryGetValue(seed.Code, out var category))
            {
                category.CategoryName = seed.Name;
                category.Description = seed.Description;
                category.SortOrder = seed.SortOrder;
                category.IsActive = true;
                category.IsDeleted = false;
                category.UpdatedAt = now;
                continue;
            }

            _dbContext.ProductCategories.Add(new ProductCategory
            {
                CategoryCode = seed.Code,
                CategoryName = seed.Name,
                Description = seed.Description,
                SortOrder = seed.SortOrder,
                IsActive = true,
                CreatedAt = now,
                IsDeleted = false
            });
        }
    }

    private async Task SeedAttributesAsync(CancellationToken cancellationToken)
    {
        var attributeCodes = StorefrontCatalogSeedData.Attributes.Select(seed => seed.Code).ToArray();
        var existingAttributes = await _dbContext.ProductAttributes
            .Where(attribute => attributeCodes.Contains(attribute.AttributeCode))
            .ToDictionaryAsync(attribute => attribute.AttributeCode, cancellationToken);

        foreach (var seed in StorefrontCatalogSeedData.Attributes)
        {
            if (existingAttributes.ContainsKey(seed.Code))
            {
                continue;
            }

            _dbContext.ProductAttributes.Add(new ProductAttribute
            {
                AttributeCode = seed.Code,
                AttributeName = seed.Name,
                InputType = seed.InputType,
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    private async Task<Warehouse> EnsureWarehouseAsync(CancellationToken cancellationToken)
    {
        var warehouse = await _dbContext.Warehouses
            .FirstOrDefaultAsync(item => item.WarehouseCode == WarehouseCode, cancellationToken);
        if (warehouse is not null)
        {
            warehouse.WarehouseName = "線上銷售倉";
            warehouse.IsActive = true;
            return warehouse;
        }

        warehouse = new Warehouse
        {
            WarehouseCode = WarehouseCode,
            WarehouseName = "線上銷售倉",
            Address = "前台可購買商品預設庫位",
            ContactName = "FreshMart",
            ContactPhone = "0000-000-000",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Warehouses.Add(warehouse);

        return warehouse;
    }

    private async Task<Product> EnsureProductAsync(
        StorefrontProductSeed seed,
        int categoryId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(item => item.ProductNo == seed.ProductNo, cancellationToken);
        if (product is null)
        {
            product = new Product
            {
                ProductNo = seed.ProductNo,
                CreatedAt = now
            };
            _dbContext.Products.Add(product);
        }

        product.CategoryId = categoryId;
        product.ProductName = seed.ProductName;
        product.BrandName = "FreshMart";
        product.ShortDescription = seed.ShortDescription;
        product.FullDescription = seed.ShortDescription;
        product.Status = "Active";
        product.IsFeatured = seed.IsFeatured;
        product.PublishedAt = seed.PublishedAt;
        product.UpdatedAt = now;
        product.IsDeleted = false;

        return product;
    }

    private async Task<ProductSku> EnsureSkuAsync(
        StorefrontProductSeed seed,
        long productId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var sku = await _dbContext.ProductSkus
            .FirstOrDefaultAsync(item => item.SkuNo == seed.SkuNo, cancellationToken);
        if (sku is null)
        {
            sku = new ProductSku
            {
                SkuNo = seed.SkuNo,
                CreatedAt = now
            };
            _dbContext.ProductSkus.Add(sku);
        }

        sku.ProductId = productId;
        sku.Barcode = seed.SkuNo;
        sku.SkuName = seed.SkuName;
        sku.SpecText = seed.UnitText;
        sku.ListPrice = seed.ListPrice;
        sku.SalePrice = seed.SalePrice;
        sku.Status = "Active";
        sku.UpdatedAt = now;
        sku.IsDeleted = false;

        return sku;
    }

    private async Task EnsureStockAsync(
        StorefrontProductSeed seed,
        int warehouseId,
        long skuId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var stock = await _dbContext.InventoryStocks
            .FirstOrDefaultAsync(
                item => item.WarehouseId == warehouseId && item.SkuId == skuId,
                cancellationToken);
        if (stock is null)
        {
            _dbContext.InventoryStocks.Add(new InventoryStock
            {
                WarehouseId = warehouseId,
                SkuId = skuId,
                OnHandQty = seed.StockQuantity,
                ReservedQty = 0,
                SafetyStockQty = Math.Min(seed.StockQuantity, 5),
                UpdatedAt = now
            });
            return;
        }

        if (stock.OnHandQty < seed.StockQuantity)
        {
            stock.OnHandQty = seed.StockQuantity;
        }

        stock.SafetyStockQty = Math.Min(seed.StockQuantity, 5);
        stock.UpdatedAt = now;
    }

    private async Task EnsureProductAttributesAsync(
        StorefrontProductSeed seed,
        long skuId,
        IReadOnlyDictionary<string, ProductAttribute> attributesByCode,
        CancellationToken cancellationToken)
    {
        var valuesByAttributeCode = new Dictionary<string, string>
        {
            ["ORIGIN"] = seed.Origin,
            ["FARMER"] = seed.Farmer,
            ["CERTIFICATION"] = string.Join(',', seed.Certifications),
            ["RATING"] = seed.Rating,
            ["LABEL"] = seed.Label,
            ["VISUAL"] = seed.VisualClass
        };

        foreach (var (attributeCode, attributeValue) in valuesByAttributeCode)
        {
            if (!attributesByCode.TryGetValue(attributeCode, out var attribute))
            {
                throw new InvalidOperationException($"Seed attribute '{attributeCode}' was not created.");
            }

            var existingValue = await _dbContext.ProductAttributeValues
                .FirstOrDefaultAsync(
                    item => item.SkuId == skuId && item.AttributeId == attribute.AttributeId,
                    cancellationToken);
            if (existingValue is null)
            {
                _dbContext.ProductAttributeValues.Add(new ProductAttributeValue
                {
                    SkuId = skuId,
                    AttributeId = attribute.AttributeId,
                    AttributeValue = attributeValue
                });
                continue;
            }

            existingValue.AttributeValue = attributeValue;
        }
    }
}

public static class StorefrontCatalogSeederExtensions
{
    public static async Task SeedStorefrontCatalogAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<StorefrontCatalogSeeder>();
        await seeder.SeedAsync();
    }
}
