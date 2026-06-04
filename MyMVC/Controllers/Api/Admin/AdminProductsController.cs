using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Catalog;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/products")]
public class AdminProductsController : ApiControllerBase
{
    private readonly IProductService _productService;

    public AdminProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult GetProducts()
    {
        return NotImplementedEndpoint("後台商品列表");
    }

    [HttpPost]
    public ActionResult CreateProduct()
    {
        return NotImplementedEndpoint("後台新增商品");
    }

    [HttpPut("{productId:long}")]
    public ActionResult UpdateProduct(long productId)
    {
        return NotImplementedEndpoint("後台修改商品");
    }

    [HttpPost("{productId:long}/publish")]
    public ActionResult PublishProduct(long productId)
    {
        return NotImplementedEndpoint("後台商品上架");
    }
}
