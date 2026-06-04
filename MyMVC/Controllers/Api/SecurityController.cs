using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs.Accounts;
using MyWeb.DTOs.Common;
using MyWeb.Extensions;

namespace MyWeb.Controllers.Api;

[ApiController]
[Route("api/security")]
public class SecurityController : ControllerBase
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
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<CsrfTokenResponse>.Failure(
                    ErrorCode.InternalServerError,
                    "無法建立 CSRF 安全權杖。"));
        }

        Response.Cookies.Append(
            AntiforgeryExtensions.ReadableCsrfCookieName,
            tokens.RequestToken,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/"
            });

        return Ok(ApiResponse<CsrfTokenResponse>.Success(new CsrfTokenResponse
        {
            HeaderName = AntiforgeryExtensions.CsrfHeaderName,
            Token = tokens.RequestToken
        }));
    }
}
