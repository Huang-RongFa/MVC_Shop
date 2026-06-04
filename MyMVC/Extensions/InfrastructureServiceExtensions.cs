using Microsoft.EntityFrameworkCore;
using MyWeb.Data;
using MyWeb.Infrastructure.Persistence;
using MyWeb.Infrastructure.Repositories.Accounts;
using MyWeb.Infrastructure.Repositories.Catalog;
using MyWeb.Infrastructure.Repositories.Inventory;
using MyWeb.Infrastructure.Repositories.Logs;
using MyWeb.Infrastructure.Repositories.Orders;
using MyWeb.Infrastructure.Repositories.Payments;
using MyWeb.Infrastructure.Repositories.Promotions;
using MyWeb.Repositories;
using MyWeb.Repositories.Accounts;
using MyWeb.Repositories.Catalog;
using MyWeb.Repositories.Inventory;
using MyWeb.Repositories.Logs;
using MyWeb.Repositories.Orders;
using MyWeb.Repositories.Payments;
using MyWeb.Repositories.Promotions;

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

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductSkuRepository, ProductSkuRepository>();
        services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();

        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();

        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IShipmentRepository, ShipmentRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();

        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<ILogRepository, LogRepository>();

        return services;
    }
}
