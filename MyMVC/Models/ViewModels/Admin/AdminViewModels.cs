namespace MyWeb.Models.ViewModels.Admin;

public sealed class AdminDashboardViewModel
{
    public string DisplayName { get; init; } = "管理員";

    public IReadOnlyList<AdminRoleDescriptionViewModel> RoleDescriptions { get; init; } = [];

    public IReadOnlyList<AdminQuickLinkViewModel> QuickLinks { get; init; } = [];

    public IReadOnlyList<AdminMetricCardViewModel> Metrics { get; init; } = [];

    public IReadOnlyList<AdminWorkItemViewModel> WorkItems { get; init; } = [];

    public IReadOnlyList<AdminTableRowViewModel> RecentOrders { get; init; } = [];

    public IReadOnlyList<AdminAuditEventViewModel> RecentAuditEvents { get; init; } = [];

    public IReadOnlyList<AdminWorkItemViewModel> SystemAlerts { get; init; } = [];
}

public sealed class AdminRoleDescriptionViewModel
{
    public string RoleCode { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;
}

public sealed class AdminQuickLinkViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Controller { get; init; } = "Admin";

    public string Action { get; init; } = string.Empty;

    public string PermissionName { get; init; } = string.Empty;
}

public sealed class AdminMetricCardViewModel
{
    public string Label { get; init; } = string.Empty;

    public string Value { get; init; } = string.Empty;

    public string Note { get; init; } = string.Empty;

    public string Variant { get; init; } = "neutral";
}

public sealed class AdminWorkItemViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string BadgeText { get; init; } = string.Empty;

    public string BadgeVariant { get; init; } = "warning";
}

public sealed class AdminAnalyticsViewModel
{
    public IReadOnlyList<AdminMetricCardViewModel> Metrics { get; init; } = [];

    public IReadOnlyList<AdminChartPointViewModel> RevenueTrend { get; init; } = [];

    public IReadOnlyList<AdminChartPointViewModel> OrderTrend { get; init; } = [];

    public IReadOnlyList<AdminRankingRowViewModel> TopSellingProducts { get; init; } = [];
}

public sealed class AdminChartPointViewModel
{
    public string Label { get; init; } = string.Empty;

    public int Percentage { get; init; }

    public string ValueText { get; init; } = string.Empty;
}

public sealed class AdminRankingRowViewModel
{
    public string Name { get; init; } = string.Empty;

    public string CategoryName { get; init; } = string.Empty;

    public string ValueText { get; init; } = string.Empty;

    public string BadgeText { get; init; } = string.Empty;
}

public sealed class AdminModulePageViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string PermissionName { get; init; } = string.Empty;

    public string PrimaryActionText { get; init; } = string.Empty;

    public bool CanUsePrimaryAction { get; init; }

    public IReadOnlyList<AdminMetricCardViewModel> Metrics { get; init; } = [];

    public IReadOnlyList<string> Columns { get; init; } = [];

    public IReadOnlyList<AdminTableRowViewModel> Rows { get; init; } = [];

    public IReadOnlyList<AdminKanbanColumnViewModel> WorkflowColumns { get; init; } = [];

    public string EmptyStateTitle { get; init; } = "目前沒有資料";

    public string EmptyStateDescription { get; init; } = "正式 Service 串接完成後，此頁會顯示即時資料。";
}

public sealed class AdminTableRowViewModel
{
    public string LeadingIcon { get; init; } = string.Empty;

    public IReadOnlyList<string> Cells { get; init; } = [];

    public string StatusText { get; init; } = string.Empty;

    public string StatusVariant { get; init; } = "neutral";

    public string ActionText { get; init; } = "查看";
}

public sealed class AdminAuditEventViewModel
{
    public string ActionName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string OccurredAtText { get; init; } = string.Empty;

    public string Variant { get; init; } = "neutral";
}

public sealed class AdminKanbanColumnViewModel
{
    public string Title { get; init; } = string.Empty;

    public IReadOnlyList<AdminWorkItemViewModel> Items { get; init; } = [];
}
