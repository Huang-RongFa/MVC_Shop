namespace MyWeb.Domain.Entities.Catalog;

public class ProductCategory
{
    public int CategoryId { get; set; }
    public int? ParentCategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ProductCategory? ParentCategory { get; set; }
    public ICollection<ProductCategory> ChildCategories { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
}

public class Product
{
    public long ProductId { get; set; }
    public string ProductNo { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? BrandName { get; set; }
    public string? ShortDescription { get; set; }
    public string? FullDescription { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public ProductCategory Category { get; set; } = null!;
    public ICollection<ProductSku> Skus { get; set; } = [];
    public ICollection<ProductImage> Images { get; set; } = [];
}

public class ProductSku
{
    public long SkuId { get; set; }
    public long ProductId { get; set; }
    public string SkuNo { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string SkuName { get; set; } = string.Empty;
    public string? SpecText { get; set; }
    public decimal ListPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? Weight { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    public Product Product { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = [];
}

public class ProductImage
{
    public long ImageId { get; set; }
    public long ProductId { get; set; }
    public long? SkuId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsMainImage { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
    public ProductSku? Sku { get; set; }
}

public class ProductAttribute
{
    public int AttributeId { get; set; }
    public string AttributeCode { get; set; } = string.Empty;
    public string AttributeName { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<ProductAttributeValue> Values { get; set; } = [];
}

public class ProductAttributeValue
{
    public long AttributeValueId { get; set; }
    public long SkuId { get; set; }
    public int AttributeId { get; set; }
    public string AttributeValue { get; set; } = string.Empty;

    public ProductSku Sku { get; set; } = null!;
    public ProductAttribute Attribute { get; set; } = null!;
}
