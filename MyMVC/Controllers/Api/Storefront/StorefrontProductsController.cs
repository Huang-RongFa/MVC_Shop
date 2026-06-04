using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs.Common;
using MyWeb.DTOs.Storefront.Products;
using MyWeb.Services.Storefront;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Route("api/storefront/products")]
public class StorefrontProductsController : ControllerBase
{
    private readonly IStorefrontProductService _productService;

    public StorefrontProductsController(IStorefrontProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListItemDto>>> SearchProducts(
        [FromQuery] ProductListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _productService.SearchProductsAsync(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{productId:long}")]
    public async Task<ActionResult<ProductDetailDto>> GetProductDetail(
        long productId,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetProductDetailAsync(productId, cancellationToken);

        if (product is null)
        {
            return NotFound();
        }

        return Ok(product);
    }
}
