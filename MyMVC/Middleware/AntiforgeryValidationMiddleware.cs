using System.Text.Json;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using MyWeb.DTOs.Common;

namespace MyWeb.Middleware;

public sealed class AntiforgeryValidationMiddleware
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Trace
    };

    private readonly RequestDelegate _next;

    public AntiforgeryValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
    {
        if (ShouldValidate(context))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json; charset=utf-8";

                var response = ApiResponse.Failure(
                    ErrorCode.ValidationFailed,
                    "CSRF 驗證失敗，請重新取得安全權杖後再送出。");

                await JsonSerializer.SerializeAsync(
                    context.Response.Body,
                    response,
                    cancellationToken: context.RequestAborted);

                return;
            }
        }

        await _next(context);
    }

    private static bool ShouldValidate(HttpContext context)
    {
        if (SafeMethods.Contains(context.Request.Method))
        {
            return false;
        }

        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            return false;
        }

        var endpoint = context.GetEndpoint();
        return endpoint?.Metadata.GetMetadata<IgnoreAntiforgeryTokenAttribute>() is null;
    }
}
