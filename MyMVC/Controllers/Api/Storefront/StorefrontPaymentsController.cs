using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Payments;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Authorize]
[Route("api/storefront/orders/{orderId:long}/payments")]
public class StorefrontPaymentsController : ApiControllerBase
{
    private readonly IPaymentService _paymentService;

    public StorefrontPaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public ActionResult CreatePayment(long orderId)
    {
        return NotImplementedEndpoint("前台建立付款");
    }
}
