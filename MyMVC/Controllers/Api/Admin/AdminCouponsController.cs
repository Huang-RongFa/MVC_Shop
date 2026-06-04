using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Promotions;
using MyWeb.Services.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Authorize(Policy = AuthPolicies.AdminAccess)]
[Route("api/admin/coupons")]
public class AdminCouponsController : ApiControllerBase
{
    private readonly ICouponService _couponService;

    public AdminCouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpGet]
    public ActionResult GetCoupons()
    {
        return NotImplementedEndpoint("後台優惠券列表");
    }
}
