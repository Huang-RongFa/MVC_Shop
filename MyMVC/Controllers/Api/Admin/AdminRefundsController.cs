using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Payments;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin")]
public class AdminRefundsController : ApiControllerBase
{
    private readonly IRefundService _refundService;

    public AdminRefundsController(IRefundService refundService)
    {
        _refundService = refundService;
    }

    [HttpGet("refunds")]
    public ActionResult GetRefunds()
    {
        return NotImplementedEndpoint("後台退款列表");
    }

    [HttpPost("orders/{orderId:long}/refunds")]
    public ActionResult CreateRefund(long orderId)
    {
        return NotImplementedEndpoint("後台建立退款");
    }
}
