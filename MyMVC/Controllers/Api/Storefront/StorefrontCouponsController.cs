using Microsoft.AspNetCore.Mvc;
using MyWeb.Services.Promotions;

namespace MyWeb.Controllers.Api.Storefront;

[ApiController]
[Route("api/storefront/coupons")]
public class StorefrontCouponsController : ApiControllerBase
{
    private readonly ICouponService _couponService;

    public StorefrontCouponsController(ICouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost("validate")]
    public ActionResult ValidateCoupon()
    {
        return NotImplementedEndpoint("前台優惠券驗證");
    }
}
