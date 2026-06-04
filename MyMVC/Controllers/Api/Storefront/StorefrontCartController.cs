using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Orders;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Authorize]
[Route("api/storefront/cart")]
public class StorefrontCartController : ApiControllerBase
{
    private readonly ICartService _cartService;

    public StorefrontCartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public ActionResult GetCart()
    {
        return NotImplementedEndpoint("前台購物車查詢");
    }

    [HttpPost("items")]
    public ActionResult AddItem()
    {
        return NotImplementedEndpoint("前台加入購物車");
    }
}
