using System.Globalization;
using MyWeb.Application.DTOs.Storefront.Products;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Services.Catalog;

/// <summary>
/// 前台商品查詢應用服務。
/// Repository 回傳偏資料查詢的投影結果，此 Service 再轉成 Razor ViewModel，
/// 讓畫面格式、價格文字與庫存狀態文字不滲入 Infrastructure 層。
/// </summary>
public sealed class ProductCatalogQueryService : IProductCatalogQueryService
{
    private const string OriginAttributeCode = "ORIGIN";
    private const string FarmerAttributeCode = "FARMER";
    private const string CertificationAttributeCode = "CERTIFICATION";
    private const string RatingAttributeCode = "RATING";
    private const string LabelAttributeCode = "LABEL";
    private const string VisualAttributeCode = "VISUAL";
    private static readonly CultureInfo TaiwanCulture = CultureInfo.GetCultureInfo("zh-TW");

    private readonly IProductCatalogRepository _productCatalogRepository;

    public ProductCatalogQueryService(IProductCatalogRepository productCatalogRepository)
    {
        _productCatalogRepository = productCatalogRepository;
    }

    public async Task<IReadOnlyList<CategoryCardViewModel>> GetCategoriesAsync(
        CancellationToken cancellationToken)
    {
        var categories = await _productCatalogRepository.GetActiveCategoriesAsync(cancellationToken);
        return categories.Select(MapCategory).ToArray();
    }

    public async Task<IReadOnlyList<ProductCardViewModel>> GetFeaturedProductsAsync(
        int take,
        CancellationToken cancellationToken)
    {
        // 首頁精選商品仍走同一條商品搜尋管線，確保上架、SKU 與庫存過濾規則一致。
        var criteria = new ProductCatalogSearchCriteria(
            Keyword: string.Empty,
            CategoryCode: string.Empty,
            OriginName: string.Empty,
            Certification: string.Empty,
            MinPrice: null,
            MaxPrice: null,
            Sort: "popular",
            Page: 1,
            PageSize: Math.Clamp(take, 1, 24),
            FeaturedOnly: true);

        var result = await _productCatalogRepository.SearchActiveProductsAsync(
            criteria,
            cancellationToken);

        return result.Items.Select(MapProductCard).ToArray();
    }

    public async Task<ProductCatalogPageResult> SearchProductsAsync(
        ProductCatalogSearchCriteria criteria,
        CancellationToken cancellationToken)
    {
        // 分頁與排序在 Service 先正規化，再交給 Repository 組 EF 查詢，避免任意排序欄位流入資料層。
        var normalizedCriteria = criteria with
        {
            Keyword = criteria.Keyword.Trim(),
            CategoryCode = criteria.CategoryCode.Trim(),
            OriginName = criteria.OriginName.Trim(),
            Certification = criteria.Certification.Trim(),
            Sort = NormalizeSort(criteria.Sort),
            Page = criteria.Page < 1 ? 1 : criteria.Page,
            PageSize = Math.Clamp(criteria.PageSize, 1, 48)
        };

        var products = await _productCatalogRepository.SearchActiveProductsAsync(
            normalizedCriteria,
            cancellationToken);
        var filters = await _productCatalogRepository.GetFilterValuesAsync(cancellationToken);

        return new ProductCatalogPageResult(
            products.TotalCount,
            filters.Origins,
            filters.Certifications,
            products.Items.Select(MapProductCard).ToArray());
    }

    public async Task<ProductDetailViewModel?> GetProductDetailAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        // 商品詳細只回傳前台可見且可販售的商品；不存在或未上架都由上層轉成 404。
        var product = await _productCatalogRepository.GetActiveProductAsync(
            productId,
            cancellationToken);
        if (product is null)
        {
            return null;
        }

        var skuOptions = await _productCatalogRepository.GetActiveSkuOptionsAsync(
            productId,
            cancellationToken);

        return new ProductDetailViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Description = product.ShortDescription,
            CategoryName = product.CategoryName,
            OriginName = GetAttribute(product, OriginAttributeCode),
            FarmerName = GetAttribute(product, FarmerAttributeCode),
            PriceRange = FormatMoney(product.SalePrice),
            UnitText = product.UnitText,
            StockStatusText = ResolveStockStatusText(product.AvailableStockQuantity),
            StockStatusVariant = ResolveStockStatusVariant(product.AvailableStockQuantity),
            AvailableStockQuantity = product.AvailableStockQuantity,
            VisualClass = GetAttribute(product, VisualAttributeCode, ResolveCategoryVisualClass(product.CategoryCode)),
            Certifications = SplitCsv(GetAttribute(product, CertificationAttributeCode)),
            // 這些提示是前台顯示文案，不代表交易規則；真正下單仍需 OrderService 重新驗證價格、庫存與付款流程。
            Highlights =
            [
                "商品價格與優惠由後端提供。",
                "建立訂單時會重新確認庫存與折扣。",
                "付款成功不代表自動出貨。"
            ],
            Skus = skuOptions.Select(sku => new ProductSkuOptionViewModel
            {
                SkuId = sku.SkuId,
                SkuName = string.IsNullOrWhiteSpace(sku.UnitText)
                    ? sku.SkuName
                    : $"{sku.SkuName} / {sku.UnitText}",
                PriceText = FormatMoney(sku.SalePrice),
                StockStatusText = ResolveStockStatusText(sku.AvailableStockQuantity)
            }).ToArray()
        };
    }

    private static CategoryCardViewModel MapCategory(ProductCatalogCategoryRecord category)
    {
        return new CategoryCardViewModel
        {
            Name = category.CategoryName,
            Description = category.Description,
            Slug = category.CategoryCode,
            VisualClass = ResolveCategoryVisualClass(category.CategoryCode)
        };
    }

    private static ProductCardViewModel MapProductCard(ProductCatalogItemRecord product)
    {
        // ProductCardViewModel 是前台展示模型，不包含成本、內部狀態或後台稽核欄位。
        return new ProductCardViewModel
        {
            ProductId = product.ProductId,
            Name = product.ProductName,
            ShortDescription = product.ShortDescription,
            CategoryName = product.CategoryName,
            OriginName = GetAttribute(product, OriginAttributeCode),
            FarmerName = GetAttribute(product, FarmerAttributeCode),
            PriceText = FormatMoney(product.SalePrice),
            PriceAmount = product.SalePrice,
            UnitText = product.UnitText,
            PromotionLabel = GetAttribute(product, LabelAttributeCode),
            StockStatusText = ResolveStockStatusText(product.AvailableStockQuantity),
            StockStatusVariant = ResolveStockStatusVariant(product.AvailableStockQuantity),
            AvailableStockQuantity = product.AvailableStockQuantity,
            VisualClass = GetAttribute(product, VisualAttributeCode, ResolveCategoryVisualClass(product.CategoryCode)),
            RatingText = GetAttribute(product, RatingAttributeCode, "0"),
            PublishedAt = product.PublishedAt ?? DateTime.MinValue,
            Certifications = SplitCsv(GetAttribute(product, CertificationAttributeCode))
        };
    }

    private static string GetAttribute(
        ProductCatalogItemRecord product,
        string attributeCode,
        string defaultValue = "")
    {
        return product.Attributes.TryGetValue(attributeCode, out var value)
            ? value
            : defaultValue;
    }

    private static IReadOnlyList<string> SplitCsv(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string NormalizeSort(string sort)
    {
        // 排序白名單集中在這裡，避免把前端 query string 直接拼成資料庫排序欄位。
        return sort switch
        {
            "new" => "new",
            "price-asc" => "price-asc",
            "price-desc" => "price-desc",
            "rating" => "rating",
            _ => "popular"
        };
    }

    private static string ResolveCategoryVisualClass(string categoryCode)
    {
        return categoryCode.ToLowerInvariant() switch
        {
            "fruit" => "visual-fruit",
            "vegetable" => "visual-vegetable",
            "chilled" => "visual-chilled",
            "promotion" => "visual-promotion",
            _ => "visual-fruit"
        };
    }

    private static string ResolveStockStatusText(int availableStockQuantity)
    {
        // 此處只負責顯示狀態；建立訂單時仍需在交易中重新檢查可售庫存。
        return availableStockQuantity switch
        {
            <= 0 => "暫無庫存",
            <= 6 => "低庫存",
            _ => "可購買"
        };
    }

    private static string ResolveStockStatusVariant(int availableStockQuantity)
    {
        return availableStockQuantity switch
        {
            <= 0 => "danger",
            <= 6 => "warning",
            _ => "success"
        };
    }

    private static string FormatMoney(decimal amount)
    {
        return string.Format(TaiwanCulture, "NT$ {0:N0}", amount);
    }
}
