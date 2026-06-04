using Microsoft.AspNetCore.Authorization;
using MyWeb.Services.Security;

namespace MyWeb.Extensions;

public static class AuthorizationExtensions
{
    public static IServiceCollection AddApplicationAuthorization(
        this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthPolicies.AdminAccess, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim(
                    AppAuthenticationDefaults.UserTypeClaimType,
                    AuthPolicies.AdminUserTypes);
            });

            foreach (var permissionCode in AuthPolicies.PermissionCodes)
            {
                options.AddPolicy(permissionCode, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new PermissionRequirement(permissionCode));
                });
            }
        });

        return services;
    }
}
