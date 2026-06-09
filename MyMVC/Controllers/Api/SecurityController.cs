using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Common;

namespace MyWeb.Controllers.Api;

[ApiController]
[Route("api/security")]
public sealed class SecurityController : ControllerBase
{
    private readonly IAntiforgery _antiforgery;

    public SecurityController(IAntiforgery antiforgery)
    {
        _antiforgery = antiforgery;
    }

    [HttpGet("csrf-token")]
    public ActionResult<ApiResponse<CsrfTokenResponse>> GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        if (string.IsNullOrWhiteSpace(tokens.RequestToken))
        {
            return BadRequest(ApiResponse.Failure("CSRF_TOKEN_UNAVAILABLE", "無法取得安全權杖，請稍後再試。"));
        }

        Response.Cookies.Append(
            "XSRF-TOKEN",
            tokens.RequestToken,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        return Ok(ApiResponse<CsrfTokenResponse>.Success(
            new CsrfTokenResponse("X-CSRF-TOKEN", tokens.RequestToken)));
    }
}

public sealed record CsrfTokenResponse(string HeaderName, string Token);
