using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Payments;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin")]
public class AdminShipmentsController : ApiControllerBase
{
    private readonly IShipmentService _shipmentService;

    public AdminShipmentsController(IShipmentService shipmentService)
    {
        _shipmentService = shipmentService;
    }

    [HttpGet("shipments")]
    public ActionResult GetShipments()
    {
        return NotImplementedEndpoint("後台出貨列表");
    }

    [HttpPost("orders/{orderId:long}/shipments")]
    public ActionResult CreateShipment(long orderId)
    {
        return NotImplementedEndpoint("後台建立出貨");
    }

    [HttpPost("shipments/{shipmentId:long}/ship")]
    public ActionResult ConfirmShipment(long shipmentId)
    {
        return NotImplementedEndpoint("後台確認出貨");
    }
}
