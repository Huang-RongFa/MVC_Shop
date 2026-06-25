using Microsoft.EntityFrameworkCore;
using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.Commerce;
using MyWeb.Data;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Logs;

namespace MyWeb.Infrastructure.Repositories.Admin.Commerce;

/// <summary>
/// 後台商務資料存取。
/// 查詢以 DTO 投影與分頁為主；商品新增/修改在單一交易中處理 Product、SKU、圖片、AuditLogs 與 AdminActionLogs。
/// </summary>
public sealed class AdminCommerceRepository : IAdminCommerceRepository
{
    private static readonly string[] DefaultProductStatuses = ["Draft", "Active", "Inactive", "Archived"];
    private static readonly string[] DefaultSkuStatuses = ["Active", "Inactive"];
    private static readonly string[] DefaultOrderStatuses = ["Pending", "Paid", "Processing", "Shipped", "Completed", "Cancelled"];
    private static readonly string[] DefaultPaymentStatuses = ["Pending", "Paid", "Failed", "Refunded", "PartialRefunded"];
    private static readonly string[] DefaultShippingStatuses = ["Pending", "Preparing", "Shipped", "Delivered", "Returned"];

    private readonly AppDbContext _dbContext;

    public AdminCommerceRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminProductListResponse> GetProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Where(product => !product.IsDeleted);

        var (normalizedPage, totalPages, totalCount, skip) = await BuildPagingAsync(
            query,
            page,
            pageSize,
            cancellationToken);

        // 後台列表只投影頁面需要的欄位，避免載入完整商品關聯造成效能與資料外洩風險。
        var rows = await query
            .OrderByDescending(product => product.UpdatedAt ?? product.CreatedAt)
            .ThenByDescending(product => product.ProductId)
            .Skip(skip)
            .Take(pageSize)
            .Select(product => new
            {
                product.ProductId,
                product.ProductNo,
                product.ProductName,
                product.BrandName,
                CategoryName = product.Category.CategoryName,
                product.Status,
                SkuCount = product.Skus.Count(sku => !sku.IsDeleted),
                ActiveSkuCount = product.Skus.Count(sku =>
                    !sku.IsDeleted && sku.Status == "Active"),
                MinSalePrice = product.Skus
                    .Where(sku => !sku.IsDeleted)
                    .Select(sku => (decimal?)sku.SalePrice)
                    .Min(),
                MaxSalePrice = product.Skus
                    .Where(sku => !sku.IsDeleted)
                    .Select(sku => (decimal?)sku.SalePrice)
                    .Max(),
                MainImageUrl = product.Images
                    .OrderByDescending(image => image.IsMainImage)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault(),
                product.CreatedAt,
                product.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var products = rows
            .Select(row => new AdminProductListItemDto(
                row.ProductId,
                row.ProductNo,
                row.ProductName,
                row.BrandName,
                row.CategoryName,
                row.Status,
                row.SkuCount,
                row.ActiveSkuCount,
                row.MinSalePrice,
                row.MaxSalePrice,
                row.MainImageUrl,
                row.CreatedAt,
                row.UpdatedAt))
            .ToArray();

        var statuses = await _dbContext.Products
            .AsNoTracking()
            .Where(product => !product.IsDeleted)
            .Select(product => product.Status)
            .Distinct()
            .OrderBy(status => status)
            .ToListAsync(cancellationToken);

        var categoryOptions = await _dbContext.ProductCategories
            .AsNoTracking()
            .Where(category => !category.IsDeleted)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.CategoryName)
            .Select(category => new AdminCategoryOptionDto(
                category.CategoryId,
                category.CategoryCode,
                category.CategoryName,
                category.IsActive))
            .ToListAsync(cancellationToken);

        return new AdminProductListResponse(
            totalCount,
            normalizedPage,
            pageSize,
            totalPages,
            products,
            NormalizeOptions(statuses, DefaultProductStatuses),
            categoryOptions);
    }

    public async Task<AdminProductEditResponse> GetProductCreateOptionsAsync(CancellationToken cancellationToken)
    {
        var categoryOptions = await GetCategoryOptionsAsync(cancellationToken);

        return new AdminProductEditResponse(
            null,
            null,
            categoryOptions.FirstOrDefault(category => category.IsActive)?.CategoryId,
            string.Empty,
            null,
            null,
            null,
            "Draft",
            false,
            new AdminProductSkuEditDto(
                null,
                null,
                string.Empty,
                null,
                0,
                0,
                null,
                null,
                null,
                null,
                "Active"),
            [],
            DefaultProductStatuses,
            DefaultSkuStatuses,
            categoryOptions);
    }

    public async Task<AdminProductEditResponse> GetProductForEditAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(item => item.Skus.Where(sku => !sku.IsDeleted))
                .ThenInclude(sku => sku.AttributeValues)
                    .ThenInclude(value => value.Attribute)
            .Include(item => item.Images)
            .FirstOrDefaultAsync(
                item => item.ProductId == productId && !item.IsDeleted,
                cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException("ADMIN_PRODUCT_NOT_FOUND");
        }

        var categoryOptions = await GetCategoryOptionsAsync(cancellationToken);
        var sku = product.Skus
            .OrderByDescending(item => item.Status == "Active")
            .ThenBy(item => item.SkuId)
            .FirstOrDefault();

        var skuDto = sku is null
            ? new AdminProductSkuEditDto(
                null,
                null,
                product.ProductName,
                null,
                0,
                0,
                null,
                null,
                null,
                null,
                "Active")
            : BuildSkuEditDto(sku);

        var images = product.Images
            .OrderByDescending(image => image.IsMainImage)
            .ThenBy(image => image.SortOrder)
            .ThenBy(image => image.ImageId)
            .Select(image => new AdminProductImageEditDto(
                image.ImageId,
                image.ImageUrl,
                image.AltText,
                image.IsMainImage,
                image.SortOrder))
            .ToArray();

        return new AdminProductEditResponse(
            product.ProductId,
            product.ProductNo,
            product.CategoryId,
            product.ProductName,
            product.BrandName,
            product.ShortDescription,
            product.FullDescription,
            product.Status,
            product.IsFeatured,
            skuDto,
            images,
            DefaultProductStatuses,
            DefaultSkuStatuses,
            categoryOptions);
    }

    public async Task<AdminProductMutationResponse> CreateProductAsync(
        long actorUserId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        // 商品建立會跨 Products、ProductSkus、ProductAttributeValues、ProductImages 與稽核表，必須同交易。
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;

            await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
            var product = new Product
            {
                ProductNo = await GenerateProductNoAsync(now, cancellationToken),
                CategoryId = request.CategoryId,
                ProductName = request.ProductName,
                BrandName = request.BrandName,
                ShortDescription = request.ShortDescription,
                FullDescription = request.FullDescription,
                Status = request.Status,
                IsFeatured = request.IsFeatured,
                PublishedAt = string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase)
                    ? now
                    : null,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var sku = await BuildNewSkuAsync(product.ProductId, request.Sku, now, cancellationToken);
            _dbContext.ProductSkus.Add(sku);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await UpsertSkuAttributesAsync(sku.SkuId, request.Sku, now, cancellationToken);
            ReplaceProductImages(product.ProductId, [], request.Images ?? [], now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // 寫入 AuditLogs 保存資料快照；AdminActionLogs 保存「誰對哪個模組做了什麼」。
            AddProductAuditLogs(
                actorUserId,
                product.ProductId,
                "Insert",
                null,
                BuildProductAuditSnapshot(product, sku, request.Images),
                requestContext,
                now);
            _dbContext.AdminActionLogs.Add(BuildActionLog(
                actorUserId,
                "Catalog",
                "CreateProduct",
                "Product",
                product.ProductId.ToString(),
                $"ProductNo: {product.ProductNo}. Reason: {request.Reason ?? "N/A"}",
                requestContext,
                now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AdminProductMutationResponse("商品已建立。", product.ProductId);
        });
    }

    public async Task<AdminProductMutationResponse> UpdateProductAsync(
        long actorUserId,
        long productId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        // 商品更新與稽核同樣包在交易中，避免商品已改但缺少可追溯紀錄。
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;

            await EnsureCategoryExistsAsync(request.CategoryId, cancellationToken);
            var product = await _dbContext.Products
                .Include(item => item.Skus.Where(sku => !sku.IsDeleted))
                    .ThenInclude(sku => sku.AttributeValues)
                        .ThenInclude(value => value.Attribute)
                .Include(item => item.Images)
                .FirstOrDefaultAsync(
                    item => item.ProductId == productId && !item.IsDeleted,
                    cancellationToken);

            if (product is null)
            {
                throw new KeyNotFoundException("ADMIN_PRODUCT_NOT_FOUND");
            }

            var sku = ResolveEditableSku(product, request.Sku);
            var oldSnapshot = BuildProductAuditSnapshot(product, sku);

            product.CategoryId = request.CategoryId;
            product.ProductName = request.ProductName;
            product.BrandName = request.BrandName;
            product.ShortDescription = request.ShortDescription;
            product.FullDescription = request.FullDescription;
            product.IsFeatured = request.IsFeatured;
            product.UpdatedAt = now;

            if (!string.Equals(product.Status, request.Status, StringComparison.OrdinalIgnoreCase))
            {
                // 首次切換為 Active 時記錄 PublishedAt；下架再上架不覆蓋原始上架時間。
                product.PublishedAt = string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase)
                    ? now
                    : product.PublishedAt;
                product.Status = request.Status;
            }

            if (sku is null)
            {
                sku = await BuildNewSkuAsync(product.ProductId, request.Sku, now, cancellationToken);
                _dbContext.ProductSkus.Add(sku);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                UpdateSku(sku, request.Sku, now);
            }

            await UpsertSkuAttributesAsync(sku.SkuId, request.Sku, now, cancellationToken);
            ReplaceProductImages(product.ProductId, product.Images.Where(image => image.SkuId is null).ToArray(), request.Images ?? [], now);
            await _dbContext.SaveChangesAsync(cancellationToken);

            AddProductAuditLogs(
                actorUserId,
                product.ProductId,
                "Update",
                oldSnapshot,
                BuildProductAuditSnapshot(product, sku, request.Images),
                requestContext,
                now);
            _dbContext.AdminActionLogs.Add(BuildActionLog(
                actorUserId,
                "Catalog",
                "UpdateProduct",
                "Product",
                product.ProductId.ToString(),
                $"ProductNo: {product.ProductNo}. Reason: {request.Reason ?? "N/A"}",
                requestContext,
                now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AdminProductMutationResponse("商品已更新。", product.ProductId);
        });
    }

    public async Task<AdminSkuListResponse> GetSkusAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.ProductSkus
            .AsNoTracking()
            .Where(sku => !sku.IsDeleted && !sku.Product.IsDeleted);

        var (normalizedPage, totalPages, totalCount, skip) = await BuildPagingAsync(
            query,
            page,
            pageSize,
            cancellationToken);

        var skus = await query
            .OrderByDescending(sku => sku.UpdatedAt ?? sku.CreatedAt)
            .ThenByDescending(sku => sku.SkuId)
            .Skip(skip)
            .Take(pageSize)
            .Select(sku => new AdminSkuListItemDto(
                sku.SkuId,
                sku.SkuNo,
                sku.Product.ProductNo,
                sku.Product.ProductName,
                sku.SkuName,
                sku.SpecText,
                sku.ListPrice,
                sku.SalePrice,
                sku.Status,
                _dbContext.InventoryStocks
                    .Where(stock => stock.SkuId == sku.SkuId)
                    .Sum(stock => (int?)stock.OnHandQty) ?? 0,
                _dbContext.InventoryStocks
                    .Where(stock => stock.SkuId == sku.SkuId)
                    .Sum(stock => (int?)stock.ReservedQty) ?? 0,
                _dbContext.InventoryStocks
                    .Where(stock => stock.SkuId == sku.SkuId)
                    .Sum(stock => (int?)stock.AvailableQty) ?? 0,
                sku.CreatedAt,
                sku.UpdatedAt))
            .ToListAsync(cancellationToken);

        var statuses = await _dbContext.ProductSkus
            .AsNoTracking()
            .Where(sku => !sku.IsDeleted)
            .Select(sku => sku.Status)
            .Distinct()
            .OrderBy(status => status)
            .ToListAsync(cancellationToken);

        return new AdminSkuListResponse(
            totalCount,
            normalizedPage,
            pageSize,
            totalPages,
            skus,
            NormalizeOptions(statuses, DefaultSkuStatuses));
    }

    public async Task<AdminInventoryListResponse> GetInventoryAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.InventoryStocks
            .AsNoTracking()
            .Where(stock => !stock.Sku.IsDeleted && !stock.Sku.Product.IsDeleted);

        var (normalizedPage, totalPages, totalCount, skip) = await BuildPagingAsync(
            query,
            page,
            pageSize,
            cancellationToken);

        var rows = await query
            .OrderBy(stock => stock.Warehouse.WarehouseCode)
            .ThenBy(stock => stock.Sku.Product.ProductName)
            .ThenBy(stock => stock.Sku.SkuNo)
            .Skip(skip)
            .Take(pageSize)
            .Select(stock => new
            {
                stock.InventoryStockId,
                stock.WarehouseId,
                stock.Warehouse.WarehouseCode,
                stock.Warehouse.WarehouseName,
                stock.SkuId,
                stock.Sku.SkuNo,
                stock.Sku.Product.ProductName,
                stock.Sku.SkuName,
                stock.OnHandQty,
                stock.ReservedQty,
                stock.AvailableQty,
                stock.SafetyStockQty,
                stock.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var stocks = rows
            .Select(row => new AdminInventoryListItemDto(
                row.InventoryStockId,
                row.WarehouseId,
                row.WarehouseCode,
                row.WarehouseName,
                row.SkuId,
                row.SkuNo,
                row.ProductName,
                row.SkuName,
                row.OnHandQty,
                row.ReservedQty,
                row.AvailableQty,
                row.SafetyStockQty,
                BuildStockStatus(row.AvailableQty, row.SafetyStockQty),
                row.UpdatedAt))
            .ToArray();

        var warehouseOptions = await _dbContext.Warehouses
            .AsNoTracking()
            .OrderBy(warehouse => warehouse.WarehouseCode)
            .Select(warehouse => new AdminWarehouseOptionDto(
                warehouse.WarehouseId,
                warehouse.WarehouseCode,
                warehouse.WarehouseName,
                warehouse.IsActive))
            .ToListAsync(cancellationToken);

        return new AdminInventoryListResponse(
            totalCount,
            normalizedPage,
            pageSize,
            totalPages,
            stocks,
            warehouseOptions);
    }

    public async Task<AdminOrderListResponse> GetOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Orders.AsNoTracking();

        var (normalizedPage, totalPages, totalCount, skip) = await BuildPagingAsync(
            query,
            page,
            pageSize,
            cancellationToken);

        var orders = await query
            .OrderByDescending(order => order.OrderedAt)
            .ThenByDescending(order => order.OrderId)
            .Skip(skip)
            .Take(pageSize)
            .Select(order => new AdminOrderListItemDto(
                order.OrderId,
                order.OrderNo,
                order.UserId,
                order.User.DisplayName,
                order.OrderStatus,
                order.PaymentStatus,
                order.ShippingStatus,
                order.TotalAmount,
                order.Items.Sum(item => (int?)item.Quantity) ?? 0,
                order.OrderedAt,
                order.PaidAt))
            .ToListAsync(cancellationToken);

        var orderStatuses = await _dbContext.Orders
            .AsNoTracking()
            .Select(order => order.OrderStatus)
            .Distinct()
            .OrderBy(status => status)
            .ToListAsync(cancellationToken);

        var paymentStatuses = await _dbContext.Orders
            .AsNoTracking()
            .Select(order => order.PaymentStatus)
            .Distinct()
            .OrderBy(status => status)
            .ToListAsync(cancellationToken);

        var shippingStatuses = await _dbContext.Orders
            .AsNoTracking()
            .Select(order => order.ShippingStatus)
            .Distinct()
            .OrderBy(status => status)
            .ToListAsync(cancellationToken);

        return new AdminOrderListResponse(
            totalCount,
            normalizedPage,
            pageSize,
            totalPages,
            orders,
            NormalizeOptions(orderStatuses, DefaultOrderStatuses),
            NormalizeOptions(paymentStatuses, DefaultPaymentStatuses),
            NormalizeOptions(shippingStatuses, DefaultShippingStatuses));
    }

    private async Task<IReadOnlyCollection<AdminCategoryOptionDto>> GetCategoryOptionsAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ProductCategories
            .AsNoTracking()
            .Where(category => !category.IsDeleted)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.CategoryName)
            .Select(category => new AdminCategoryOptionDto(
                category.CategoryId,
                category.CategoryCode,
                category.CategoryName,
                category.IsActive))
            .ToListAsync(cancellationToken);
    }

    private static AdminProductSkuEditDto BuildSkuEditDto(ProductSku sku)
    {
        var attributeValues = sku.AttributeValues
            .GroupBy(value => value.Attribute.AttributeCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group.First().AttributeValue,
                StringComparer.OrdinalIgnoreCase);

        return new AdminProductSkuEditDto(
            sku.SkuId,
            sku.SkuNo,
            sku.SkuName,
            sku.SpecText,
            sku.ListPrice,
            sku.SalePrice,
            sku.CostPrice,
            attributeValues.GetValueOrDefault("SIZE"),
            attributeValues.GetValueOrDefault("COLOR"),
            attributeValues.GetValueOrDefault("CAPACITY"),
            sku.Status);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.ProductCategories
            .AnyAsync(
                category => category.CategoryId == categoryId
                    && category.IsActive
                    && !category.IsDeleted,
                cancellationToken);

        if (!exists)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_CATEGORY_INVALID", "商品分類不存在或未啟用。");
        }
    }

    private async Task<ProductSku> BuildNewSkuAsync(
        long productId,
        AdminProductSkuUpsertRequest request,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var skuNo = string.IsNullOrWhiteSpace(request.SkuNo)
            ? await GenerateSkuNoAsync(now, cancellationToken)
            : request.SkuNo;

        return new ProductSku
        {
            ProductId = productId,
            SkuNo = skuNo!,
            SkuName = request.SkuName,
            SpecText = request.SpecText,
            ListPrice = request.ListPrice,
            SalePrice = request.SalePrice,
            CostPrice = request.BasePrice,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
    }

    private static ProductSku? ResolveEditableSku(Product product, AdminProductSkuUpsertRequest request)
    {
        if (request.SkuId is long skuId)
        {
            return product.Skus.FirstOrDefault(sku => sku.SkuId == skuId);
        }

        return product.Skus
            .OrderByDescending(sku => sku.Status == "Active")
            .ThenBy(sku => sku.SkuId)
            .FirstOrDefault();
    }

    private static void UpdateSku(ProductSku sku, AdminProductSkuUpsertRequest request, DateTime now)
    {
        sku.SkuNo = string.IsNullOrWhiteSpace(request.SkuNo) ? sku.SkuNo : request.SkuNo!;
        sku.SkuName = request.SkuName;
        sku.SpecText = request.SpecText;
        sku.ListPrice = request.ListPrice;
        sku.SalePrice = request.SalePrice;
        sku.CostPrice = request.BasePrice;
        sku.Status = request.Status;
        sku.UpdatedAt = now;
        sku.IsDeleted = false;
    }

    private async Task UpsertSkuAttributesAsync(
        long skuId,
        AdminProductSkuUpsertRequest sku,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var attributes = new (string Code, string Name, string InputType, string? Value)[]
        {
            ("SIZE", "尺寸", "Select", sku.Size),
            ("COLOR", "顏色", "Select", sku.Color),
            ("CAPACITY", "容量", "Text", sku.Capacity)
        };

        foreach (var (code, name, inputType, value) in attributes)
        {
            var attribute = await EnsureProductAttributeAsync(code, name, inputType, now, cancellationToken);
            var existingValue = await _dbContext.ProductAttributeValues
                .FirstOrDefaultAsync(
                    item => item.SkuId == skuId && item.AttributeId == attribute.AttributeId,
                    cancellationToken);

            if (string.IsNullOrWhiteSpace(value))
            {
                if (existingValue is not null)
                {
                    _dbContext.ProductAttributeValues.Remove(existingValue);
                }

                continue;
            }

            if (existingValue is null)
            {
                _dbContext.ProductAttributeValues.Add(new ProductAttributeValue
                {
                    SkuId = skuId,
                    AttributeId = attribute.AttributeId,
                    AttributeValue = value
                });
                continue;
            }

            existingValue.AttributeValue = value;
        }
    }

    private async Task<ProductAttribute> EnsureProductAttributeAsync(
        string code,
        string name,
        string inputType,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // 規格屬性用代碼去重，避免不同商品建立重複的 SIZE/COLOR/CAPACITY 定義。
        var attribute = await _dbContext.ProductAttributes
            .FirstOrDefaultAsync(item => item.AttributeCode == code, cancellationToken);

        if (attribute is not null)
        {
            return attribute;
        }

        attribute = new ProductAttribute
        {
            AttributeCode = code,
            AttributeName = name,
            InputType = inputType,
            CreatedAt = now
        };
        _dbContext.ProductAttributes.Add(attribute);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return attribute;
    }

    private void ReplaceProductImages(
        long productId,
        IReadOnlyCollection<ProductImage> existingImages,
        IReadOnlyCollection<AdminProductImageUpsertRequest> requestedImages,
        DateTime now)
    {
        // 目前商品圖採整組替換，邏輯簡單且稽核快照完整；大量圖片情境可再改為差異更新。
        if (existingImages.Count > 0)
        {
            _dbContext.ProductImages.RemoveRange(existingImages);
        }

        foreach (var image in requestedImages.OrderBy(item => item.SortOrder))
        {
            _dbContext.ProductImages.Add(new ProductImage
            {
                ProductId = productId,
                ImageUrl = image.ImageUrl,
                AltText = image.AltText,
                IsMainImage = image.IsMainImage,
                SortOrder = image.SortOrder,
                CreatedAt = now
            });
        }
    }

    private async Task<string> GenerateProductNoAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var index = 0; index < 10; index++)
        {
            var productNo = $"PRD{now:yyyyMMddHHmmssfff}{index}";
            var exists = await _dbContext.Products.AnyAsync(product => product.ProductNo == productNo, cancellationToken);
            if (!exists)
            {
                return productNo;
            }
        }

        throw new InvalidOperationException("Unable to generate product number.");
    }

    private async Task<string> GenerateSkuNoAsync(DateTime now, CancellationToken cancellationToken)
    {
        for (var index = 0; index < 10; index++)
        {
            var skuNo = $"SKU{now:yyyyMMddHHmmssfff}{index}";
            var exists = await _dbContext.ProductSkus.AnyAsync(sku => sku.SkuNo == skuNo, cancellationToken);
            if (!exists)
            {
                return skuNo;
            }
        }

        throw new InvalidOperationException("Unable to generate sku number.");
    }

    private void AddProductAuditLogs(
        long actorUserId,
        long productId,
        string actionType,
        string? oldValueJson,
        string? newValueJson,
        AdminSystemRequestContext requestContext,
        DateTime now)
    {
        _dbContext.AuditLogs.Add(new AuditLog
        {
            TableName = "Products",
            RecordId = productId.ToString(),
            ActionType = actionType,
            OldValueJson = oldValueJson,
            NewValueJson = newValueJson,
            ChangedBy = actorUserId,
            ChangedAt = now,
            IpAddress = requestContext.IpAddress
        });
    }

    private static AdminActionLog BuildActionLog(
        long actorUserId,
        string moduleName,
        string actionName,
        string targetType,
        string targetId,
        string description,
        AdminSystemRequestContext requestContext,
        DateTime now)
    {
        return new AdminActionLog
        {
            UserId = actorUserId,
            ModuleName = moduleName,
            ActionName = actionName,
            TargetType = targetType,
            TargetId = targetId,
            Description = description,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent,
            CreatedAt = now
        };
    }

    private static string BuildProductAuditSnapshot(
        Product product,
        ProductSku? sku,
        IReadOnlyCollection<AdminProductImageUpsertRequest>? requestedImages = null)
    {
        return global::System.Text.Json.JsonSerializer.Serialize(new
        {
            product.ProductId,
            product.ProductNo,
            product.CategoryId,
            product.ProductName,
            product.BrandName,
            product.ShortDescription,
            product.FullDescription,
            product.Status,
            product.IsFeatured,
            Sku = sku is null
                ? null
                : new
                {
                    sku.SkuId,
                    sku.SkuNo,
                    sku.SkuName,
                    sku.SpecText,
                    sku.ListPrice,
                    sku.SalePrice,
                    BasePrice = sku.CostPrice,
                    sku.Status
                },
            Images = requestedImages is not null
                ? requestedImages
                    .OrderByDescending(image => image.IsMainImage)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => new
                    {
                        image.ImageUrl,
                        image.AltText,
                        image.IsMainImage,
                        image.SortOrder
                    })
                    .ToArray()
                : product.Images
                    .Where(image => image.SkuId is null)
                    .OrderByDescending(image => image.IsMainImage)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => new
                    {
                        image.ImageUrl,
                        image.AltText,
                        image.IsMainImage,
                        image.SortOrder
                    })
                    .ToArray()
        });
    }

    private static async Task<(int Page, int TotalPages, int TotalCount, int Skip)> BuildPagingAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)pageSize);
        // 超出頁碼時夾到最後一頁，避免 Skip 過大導致空頁與使用者困惑。
        var normalizedPage = Math.Min(Math.Max(page, 1), totalPages);
        var skip = (normalizedPage - 1) * pageSize;

        return (normalizedPage, totalPages, totalCount, skip);
    }

    private static IReadOnlyCollection<string> NormalizeOptions(
        IReadOnlyCollection<string> values,
        IReadOnlyCollection<string> fallbackValues)
    {
        var options = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value)
            .ToArray();

        return options.Length == 0 ? fallbackValues : options;
    }

    private static string BuildStockStatus(int availableQty, int safetyStockQty)
    {
        if (availableQty <= 0)
        {
            return "OutOfStock";
        }

        if (availableQty <= safetyStockQty)
        {
            return "LowStock";
        }

        return "InStock";
    }
}
