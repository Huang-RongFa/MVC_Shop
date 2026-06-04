using Microsoft.AspNetCore.Authorization;

namespace MyWeb.Services.Security;

public static class AppAuthenticationDefaults
{
    public const string AuthenticationScheme = "MySystemCookie";
    public const string PermissionClaimType = "permission";
    public const string UserTypeClaimType = "user_type";
}

public static class AuthPolicies
{
    public const string AdminAccess = "AdminAccess";

    public static readonly string[] AdminUserTypes = ["Admin", "Staff"];

    public static readonly string[] PermissionCodes =
    [
        "Products.Read",
        "Products.Write",
        "Inventory.Adjust",
        "Orders.Read",
        "Orders.Manage",
        "Payments.Read",
        "Shipments.Manage",
        "Refunds.Manage",
        "Coupons.Manage",
        "Users.Manage",
        "Roles.Manage",
        "AuditLogs.Read"
    ];
}

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public string PermissionCode { get; }
}

public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(claim =>
            claim.Type == AppAuthenticationDefaults.PermissionClaimType &&
            string.Equals(claim.Value, requirement.PermissionCode, StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
