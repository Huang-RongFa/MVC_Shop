using System.Threading.RateLimiting;

namespace MyWeb.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddApplicationRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var partitionKey = GetPartitionKey(context);
                var path = context.Request.Path;

                if (path.StartsWithSegments("/api/storefront/auth/login") ||
                    path.StartsWithSegments("/api/admin/auth/login"))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"auth:{partitionKey}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                }

                if (path.StartsWithSegments("/api/admin"))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        $"admin:{partitionKey}",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                }

                return RateLimitPartition.GetFixedWindowLimiter(
                    $"api:{partitionKey}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 300,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }

    private static string GetPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            return $"user:{userId}";
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}
