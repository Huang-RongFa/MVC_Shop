namespace MyWeb.Extensions;

public static class CorsExtensions
{
    public const string DefaultCorsPolicyName = "TrustedFrontendOrigins";

    public static IServiceCollection AddTrustedFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var allowedOrigins = configuration
            .GetSection("Security:Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        if (environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            allowedOrigins =
            [
                "http://localhost:5173",
                "https://localhost:5173",
                "http://localhost:5174",
                "https://localhost:5174"
            ];
        }

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    policy.SetIsOriginAllowed(_ => false);
                }
                else
                {
                    policy.WithOrigins(allowedOrigins);
                }

                policy
                    .AllowCredentials()
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .WithHeaders("Content-Type", "X-CSRF-TOKEN", "X-XSRF-TOKEN");
            });
        });

        return services;
    }
}
