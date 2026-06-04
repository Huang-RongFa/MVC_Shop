namespace MyWeb.Extensions;

public static class AntiforgeryExtensions
{
    public const string CsrfHeaderName = "X-CSRF-TOKEN";
    public const string ReadableCsrfCookieName = "XSRF-TOKEN";

    public static IServiceCollection AddApplicationAntiforgery(
        this IServiceCollection services)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = CsrfHeaderName;
            options.Cookie.Name = "__Host-MySystemCsrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.Path = "/";
        });

        return services;
    }
}
