using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MyWeb.Application.Interfaces.Admin.Commerce;
using MyWeb.Application.Interfaces.Admin.System;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Application.Services.Admin.Commerce;
using MyWeb.Application.Services.Admin.System;
using MyWeb.Application.Services.Auth;
using MyWeb.Application.Services.Cart;
using MyWeb.Application.Services.Catalog;
using MyWeb.Application.Services.Pages;
using MyWeb.Data;
using MyWeb.Data.Seed;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.Infrastructure.Repositories.Admin.Commerce;
using MyWeb.Infrastructure.Repositories.Admin.System;
using MyWeb.Infrastructure.Repositories.Auth;
using MyWeb.Infrastructure.Repositories.Catalog;
using MyWeb.Infrastructure.Repositories.Cart;

namespace MyWeb.Extensions;

/// <summary>
/// 集中註冊 Infrastructure 與 Application 層服務。
/// Program.cs 只保留安全管線與路由組態，避免啟動檔膨脹成服務定位中心。
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// 註冊資料庫、Repository、Service、密碼雜湊與開發種子資料。
    /// 這裡只描述組態與生命週期，不放任何商業規則或資料初始化流程。
    /// </summary>
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

        // EF Core DbContext 採 Scoped，讓單一 HTTP request 內的 Service/Repository 共用同一個追蹤範圍。
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

        // Repository 只處理資料存取；Service 才能決定權限、流程與交易規則。
        services.AddScoped<IUserAuthRepository, UserAuthRepository>();
        services.AddScoped<IProductCatalogRepository, ProductCatalogRepository>();
        services.AddScoped<IProductCatalogQueryService, ProductCatalogQueryService>();
        services.AddScoped<IProductStockRepository, ProductStockRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IAdminCommerceRepository, AdminCommerceRepository>();
        services.AddScoped<IAdminCommerceService, AdminCommerceService>();
        services.AddScoped<IAdminSystemRepository, AdminSystemRepository>();
        services.AddScoped<IAdminSystemService, AdminSystemService>();
        services.AddScoped<IStorefrontAuthService, StorefrontAuthService>();
        services.AddScoped<IAdminAuthService, AdminAuthService>();
        services.AddScoped<IStorefrontPageService, StorefrontPageService>();
        services.AddScoped<IAdminPageService, AdminPageService>();
        services.AddScoped<IAuthPageService, AuthPageService>();

        // 使用 ASP.NET Core Identity PasswordHasher，避免自行實作不安全的密碼雜湊。
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        // Seeder 僅供開發環境由 Program.cs 呼叫，正式環境不可依賴預設資料或預設密碼。
        services.AddScoped<AdminAuthSeeder>();
        services.AddScoped<StorefrontCatalogSeeder>();

        return services;
    }
}
