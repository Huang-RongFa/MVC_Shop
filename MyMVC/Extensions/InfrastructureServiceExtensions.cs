using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Application.Services.Auth;
using MyWeb.Application.Services.Pages;
using MyWeb.Data;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.Infrastructure.Repositories.Auth;

namespace MyWeb.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
        });

        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IStorefrontAuthService, StorefrontAuthService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IStorefrontPageService, StorefrontPageService>();
        services.AddScoped<IAdminPageService, AdminPageService>();
        services.AddScoped<IAuthPageService, AuthPageService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        return services;
    }
}
