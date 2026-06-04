using MyWeb.DTOs.Common;

namespace MyWeb.DTOs.Storefront.Products;

public class ProductListQuery : PagedRequest
{
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }

    public string? NormalizedKeyword => string.IsNullOrWhiteSpace(Keyword)
        ? null
        : Keyword.Trim();

    public string NormalizedSortBy => string.IsNullOrWhiteSpace(SortBy)
        ? "featured"
        : SortBy.Trim().ToLowerInvariant();
}
