using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using MyWeb.DTOs.Common;
using MyWeb.Middleware;

namespace MyWeb.Extensions;

public static class SecurityPipelineExtensions
{
    public static IApplicationBuilder UseApiExceptionHandler(
        this IApplicationBuilder app,
        IWebHostEnvironment environment)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json; charset=utf-8";

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var message = environment.IsDevelopment()
                    ? feature?.Error.Message ?? "Unhandled exception."
                    : "系統發生未預期錯誤，請稍後再試。";

                await context.Response.WriteAsJsonAsync(
                    ApiResponse.Failure(ErrorCode.InternalServerError, message));
            });
        });

        return app;
    }

    public static IApplicationBuilder UseApplicationForwardedHeaders(
        this IApplicationBuilder app)
    {
        return app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
    }

    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static IApplicationBuilder UseApplicationAntiforgeryValidation(
        this IApplicationBuilder app)
    {
        return app.UseMiddleware<AntiforgeryValidationMiddleware>();
    }
}
