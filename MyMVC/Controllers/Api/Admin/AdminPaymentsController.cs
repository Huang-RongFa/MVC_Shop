using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Payments;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/payments")]
public class AdminPaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public AdminPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpGet]
    public ActionResult GetPayments()
    {
        return NotImplementedEndpoint("後台付款列表");
    }

    [HttpPost("{paymentId:long}/callback")]
    public ActionResult ReceiveCallback(long paymentId)
    {
        return NotImplementedEndpoint("後台金流回呼");
    }
}
