namespace MyWeb.DTOs.Storefront.Products;

public class ProductDetailDto
{
    public long ProductId { get; set; }
    public string ProductNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public IReadOnlyList<ProductImageDto> Images { get; set; } = [];
    public IReadOnlyList<ProductSkuOptionDto> Skus { get; set; } = [];
}

public class ProductImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
}

public class ProductSkuOptionDto
{
    public long SkuId { get; set; }
    public string SkuNo { get; set; } = string.Empty;
    public string SkuName { get; set; } = string.Empty;
    public string? SpecText { get; set; }
    public decimal ListPrice { get; set; }
    public decimal SalePrice { get; set; }
}
