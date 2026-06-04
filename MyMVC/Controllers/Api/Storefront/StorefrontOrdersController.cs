using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Orders;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Authorize]
[Route("api/storefront")]
public class StorefrontOrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public StorefrontOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost("orders")]
    public ActionResult CreateOrder()
    {
        return NotImplementedEndpoint("前台建立訂單");
    }

    [HttpGet("me/orders")]
    public ActionResult GetMyOrders()
    {
        return NotImplementedEndpoint("前台會員訂單列表");
    }

    [HttpGet("me/orders/{orderId:long}")]
    public ActionResult GetMyOrderDetail(long orderId)
    {
        return NotImplementedEndpoint("前台會員訂單詳細");
    }
}
