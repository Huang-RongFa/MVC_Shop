namespace MyWeb.DTOs.Storefront.Products;

public class ProductListItemDto
{
    public long ProductId { get; set; }
    public string ProductNo { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? ShortDescription { get; set; }
    public string? CategoryName { get; set; }
    public string? MainImageUrl { get; set; }
    public decimal MinSalePrice { get; set; }
    public bool IsFeatured { get; set; }
}
