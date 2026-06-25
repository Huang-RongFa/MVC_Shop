namespace MyWeb.Application.Security;

public static class AdminPermissionCodes
{
    public const string DashboardRead = "Dashboard.Read";
    public const string AnalyticsRead = "Analytics.Read";
    public const string UsersManage = "Users.Manage";
    public const string RolesManage = "Roles.Manage";
    public const string ProductsRead = "Products.Read";
    public const string ProductsWrite = "Products.Write";
    public const string InventoryRead = "Inventory.Read";
    public const string InventoryAdjust = "Inventory.Adjust";
    public const string OrdersRead = "Orders.Read";
    public const string OrdersWrite = "Orders.Write";
    public const string PaymentsRead = "Payments.Read";
    public const string ShipmentsManage = "Shipments.Manage";
    public const string RefundsManage = "Refunds.Manage";
    public const string CouponsManage = "Coupons.Manage";
    public const string PromotionsManage = "Promotions.Manage";
    public const string MessagesRead = "Messages.Read";
    public const string AuditLogsRead = "AuditLogs.Read";
    public const string SettingsManage = "Settings.Manage";

    public static readonly string[] All =
    [
        DashboardRead,
        AnalyticsRead,
        UsersManage,
        RolesManage,
        ProductsRead,
        ProductsWrite,
        InventoryRead,
        InventoryAdjust,
        OrdersRead,
        OrdersWrite,
        PaymentsRead,
        ShipmentsManage,
        RefundsManage,
        CouponsManage,
        PromotionsManage,
        MessagesRead,
        AuditLogsRead,
        SettingsManage
    ];
}
