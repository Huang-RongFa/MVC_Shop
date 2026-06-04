using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Orders;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/orders")]
public class AdminOrdersController : ApiControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public ActionResult GetOrders()
    {
        return NotImplementedEndpoint("後台訂單列表");
    }

    [HttpGet("{orderId:long}")]
    public ActionResult GetOrderDetail(long orderId)
    {
        return NotImplementedEndpoint("後台訂單詳細");
    }

    [HttpPatch("{orderId:long}/status")]
    public ActionResult UpdateOrderStatus(long orderId)
    {
        return NotImplementedEndpoint("後台修改訂單狀態");
    }
}
