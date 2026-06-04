using MyWeb.Services.Accounts;
using MyWeb.Services.Catalog;
using MyWeb.Services.Inventory;
using MyWeb.Services.Logs;
using MyWeb.Services.Orders;
using MyWeb.Services.Payments;
using MyWeb.Services.Promotions;
using MyWeb.Services.Security;
using MyWeb.Services.Storefront;
using Microsoft.AspNetCore.Identity;
using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductSkuService, ProductSkuService>();
        services.AddScoped<IProductCategoryService, ProductCategoryService>();
        services.AddScoped<IStorefrontProductService, StorefrontProductService>();

        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IWarehouseService, WarehouseService>();

        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();

        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IRefundService, RefundService>();

        services.AddScoped<ICouponService, CouponService>();
        services.AddScoped<IPromotionService, PromotionService>();

        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<ILogService, LogService>();

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<IClockService, ClockService>();

        return services;
    }
}
