using Microsoft.AspNetCore.Authentication.Cookies;
using MyWeb.Services.Security;

namespace MyWeb.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddApplicationCookieAuthentication(
        this IServiceCollection services)
    {
        services
            .AddAuthentication(AppAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(AppAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "__Host-MySystemAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.Path = "/";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}
