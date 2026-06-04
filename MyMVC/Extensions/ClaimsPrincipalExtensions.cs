using System.Security.Claims;

namespace MyWeb.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static long? GetUserId(this ClaimsPrincipal principal)
    {
        var rawUserId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(rawUserId, out var userId) ? userId : null;
    }
}
