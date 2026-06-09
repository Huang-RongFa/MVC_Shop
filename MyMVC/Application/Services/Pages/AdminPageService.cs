using System.Security.Claims;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models.ViewModels.Admin;

namespace MyWeb.Application.Services.Pages;

/// <summary>
/// 組裝後台 Razor ViewModel。
/// 目前用於呈現管理頁骨架；正式環境應由各模組 Service 提供資料並在 Service 層檢查權限。
/// </summary>
public sealed class AdminPageService : IAdminPageService
{
    public Task<AdminDashboardViewModel> GetDashboardAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // TODO: 改由 DashboardService 彙整 Orders / Payments / Shipments / InventoryStocks，並依 Dashboard.Read 權限檢查。
        return Task.FromResult(new AdminDashboardViewModel
        {
            DisplayName = user.Identity?.Name ?? "管理員",
            Metrics =
            [
                Metric("今日營收", "待串接", "資料來源：Orders / Payments", "neutral"),
                Metric("今日訂單", "待串接", "資料來源：Orders", "neutral"),
                Metric("待出貨", "待串接", "資料來源：Shipments", "warning"),
                Metric("低庫存 SKU", "待串接", "資料來源：InventoryStocks", "danger")
            ],
            WorkItems =
            [
                WorkItem("確認付款完成訂單", "付款成功只代表 Paid，不等於自動出貨。", "Orders", "warning"),
                WorkItem("檢查低庫存商品", "出貨確認後才扣實際庫存，調整需留下紀錄。", "Inventory", "danger"),
                WorkItem("審核退款申請", "退款支援整筆與部分退款，需保留稽核紀錄。", "Refunds", "warning")
            ],
            SystemAlerts =
            [
                WorkItem("後台操作紀錄", "權限異動、庫存調整、退款與訂單狀態修改需寫入稽核。", "Audit", "success"),
                WorkItem("正式資料來源", "目前頁面資料集中在 ViewModel 服務，後續可替換正式 Service。", "Service", "neutral")
            ]
        });
    }

    public Task<AdminAnalyticsViewModel> GetAnalyticsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // TODO: 改由 AnalyticsService 查詢正式統計資料；避免前台或無權限後台帳號看到營收、轉換率等敏感指標。
        return Task.FromResult(new AdminAnalyticsViewModel
        {
            Metrics =
            [
                Metric("營收趨勢", "待串接", "需 Analytics.Read", "neutral"),
                Metric("付款成功率", "待串接", "資料來源：Payments", "success"),
                Metric("退款率", "待串接", "資料來源：Refunds", "warning"),
                Metric("會員成長", "待串接", "資料來源：Users", "neutral")
            ],
            RevenueTrend =
            [
                Chart("週一", 42, "待串接"),
                Chart("週二", 58, "待串接"),
                Chart("週三", 76, "待串接"),
                Chart("週四", 64, "待串接"),
                Chart("週五", 82, "待串接"),
                Chart("週六", 70, "待串接"),
                Chart("週日", 48, "待串接")
            ],
            OrderTrend =
            [
                Chart("待付款", 35, "待串接"),
                Chart("已付款", 72, "待串接"),
                Chart("待出貨", 50, "待串接"),
                Chart("已出貨", 44, "待串接"),
                Chart("退款中", 18, "待串接")
            ],
            TopSellingProducts =
            [
                new()
                {
                    Name = "正式商品資料待串接",
                    CategoryName = "ProductService",
                    ValueText = "待串接",
                    BadgeText = "Products"
                }
            ]
        });
    }

    public Task<AdminModulePageViewModel> GetModulePageAsync(
        ClaimsPrincipal user,
        string moduleKey,
        CancellationToken cancellationToken)
    {
        // TODO: 目前 moduleKey 只決定頁面骨架；正式功能需接 PermissionChecker，並依模組呼叫專責 Service。
        return Task.FromResult(moduleKey switch
        {
            "users" => Module(
                "使用者管理",
                "管理前台會員、後台員工與帳號狀態。",
                "Users.Manage",
                "新增使用者",
                ["姓名", "Email", "身分", "狀態"],
                [Metric("會員數", "待串接", "Users", "neutral"), Metric("停用帳號", "待串接", "Users", "warning")]),
            "roles" => Module(
                "角色權限管理",
                "維護角色、權限與後台選單可見範圍。",
                "Roles.Manage",
                "新增角色",
                ["角色", "權限數", "使用者數", "狀態"],
                [Metric("角色數", "待串接", "Roles", "neutral"), Metric("權限數", "待串接", "Permissions", "neutral")]),
            "products" => Module(
                "商品管理",
                "維護商品主檔、上下架狀態、分類與商品圖片。",
                "Products.Read",
                "新增商品",
                ["商品", "分類", "SKU", "狀態"],
                [Metric("已上架", "待串接", "Products", "success"), Metric("草稿", "待串接", "Products", "neutral")],
                ProductWorkflow()),
            "product-edit" => Module(
                "商品新增修改",
                "新增或修改商品資料，正式儲存時需寫入後台操作紀錄。",
                "Products.Write",
                "儲存商品",
                ["欄位", "狀態", "資料來源", "備註"],
                [Metric("編輯模式", "待串接", "ProductEdit", "neutral")]),
            "product-skus" => Module(
                "SKU 管理",
                "維護商品規格、售價、可售狀態與庫存對應。",
                "Products.Write",
                "新增 SKU",
                ["SKU", "規格", "售價", "狀態"],
                [Metric("SKU 數", "待串接", "ProductSkus", "neutral")]),
            "inventory" => Module(
                "庫存管理",
                "查詢庫存、保留庫存與庫存異動；調整庫存需有稽核紀錄。",
                "Inventory.Read",
                "調整庫存",
                ["SKU", "倉庫", "可售", "保留"],
                [Metric("低庫存", "待串接", "InventoryStocks", "danger"), Metric("保留庫存", "待串接", "InventoryReservations", "warning")]),
            "orders" => Module(
                "訂單管理",
                "查詢訂單狀態、付款狀態與出貨狀態。",
                "Orders.Read",
                "更新狀態",
                ["訂單編號", "會員", "金額", "狀態"],
                [Metric("待付款", "待串接", "Orders", "warning"), Metric("已付款", "待串接", "Orders", "success")],
                OrderWorkflow()),
            "order-details" => Module(
                "訂單詳細",
                "顯示訂單主檔、明細、付款、出貨、退款與狀態歷史。",
                "Orders.Read",
                "更新訂單",
                ["區塊", "資料來源", "狀態", "備註"],
                [Metric("訂單狀態", "待串接", "Orders", "neutral")]),
            "payments" => Module(
                "付款管理",
                "查詢付款紀錄與金流交易狀態，Callback 必須驗簽且具備冪等處理。",
                "Payments.Read",
                "查看交易",
                ["付款單", "訂單", "金額", "狀態"],
                [Metric("付款成功", "待串接", "Payments", "success"), Metric("付款失敗", "待串接", "PaymentTransactions", "danger")]),
            "shipments" => Module(
                "出貨管理",
                "管理出貨流程；出貨確認後才扣實際庫存。",
                "Shipments.Manage",
                "建立出貨",
                ["出貨單", "訂單", "物流", "狀態"],
                [Metric("待出貨", "待串接", "Shipments", "warning"), Metric("已出貨", "待串接", "Shipments", "success")]),
            "refunds" => Module(
                "退款管理",
                "處理整筆退款與部分退款，所有操作需寫入稽核紀錄。",
                "Refunds.Manage",
                "建立退款",
                ["退款單", "訂單", "金額", "狀態"],
                [Metric("待審核", "待串接", "Refunds", "warning"), Metric("已退款", "待串接", "Refunds", "success")]),
            "coupons" => Module(
                "優惠券管理",
                "維護優惠券規則、使用限制與使用紀錄。",
                "Coupons.Manage",
                "新增優惠券",
                ["優惠券", "折扣", "期間", "狀態"],
                [Metric("有效優惠券", "待串接", "Coupons", "success")]),
            "promotions" => Module(
                "促銷管理",
                "維護促銷活動與商品範圍；折扣結果需記錄來源。",
                "Promotions.Manage",
                "新增促銷",
                ["促銷", "商品範圍", "期間", "狀態"],
                [Metric("進行中", "待串接", "Promotions", "success")]),
            "inbox" => Module(
                "訊息 / 通知",
                "集中查看客服訊息、系統通知與營運警示。",
                "Messages.Read",
                "建立通知",
                ["來源", "主旨", "時間", "狀態"],
                [Metric("未讀訊息", "待串接", "Messages", "warning")]),
            "audit-logs" => Module(
                "稽核紀錄",
                "查詢後台重要操作與資料異動紀錄。",
                "AuditLogs.Read",
                "匯出紀錄",
                ["時間", "操作者", "動作", "結果"],
                [Metric("今日操作", "待串接", "AdminActionLogs", "neutral")]),
            "settings" => Module(
                "系統設定",
                "管理後台偏好、通知設定與安全設定。",
                "Settings.Manage",
                "儲存設定",
                ["設定", "目前值", "風險", "狀態"],
                [Metric("安全設定", "待串接", "Settings", "neutral")]),
            _ => Module(
                "後台管理",
                "此頁尚未定義模組。",
                "Dashboard.Read",
                string.Empty,
                ["項目", "資料來源", "狀態", "備註"],
                [])
        });
    }

    private static AdminModulePageViewModel Module(
        string title,
        string description,
        string permission,
        string primaryActionText,
        IReadOnlyList<string> columns,
        IReadOnlyList<AdminMetricCardViewModel> metrics,
        IReadOnlyList<AdminKanbanColumnViewModel>? workflowColumns = null)
    {
        // TODO: Rows 目前保留空集合；正式串接後由各模組 Service 回傳分頁資料，並保留必要 AuditLog / AdminActionLog。
        return new AdminModulePageViewModel
        {
            Title = title,
            Description = description,
            PermissionName = permission,
            PrimaryActionText = primaryActionText,
            CanUsePrimaryAction = !string.IsNullOrWhiteSpace(primaryActionText),
            Columns = columns,
            Metrics = metrics,
            WorkflowColumns = workflowColumns ?? [],
            Rows = [],
            EmptyStateTitle = "尚未串接正式資料",
            EmptyStateDescription = "此頁已完成 ViewModel 與版面骨架，後續可替換為正式 Service 查詢。"
        };
    }

    private static IReadOnlyList<AdminKanbanColumnViewModel> ProductWorkflow()
    {
        // TODO: 商品流程需接 ProductService，依草稿、審核、上架、下架狀態查詢正式資料。
        return
        [
            new() { Title = "草稿", Items = [WorkItem("商品草稿", "等待 Products.Write 串接。", "Draft", "neutral")] },
            new() { Title = "待審核", Items = [] },
            new() { Title = "已上架", Items = [] },
            new() { Title = "已下架", Items = [] }
        ];
    }

    private static IReadOnlyList<AdminKanbanColumnViewModel> OrderWorkflow()
    {
        // TODO: 訂單流程需接 OrderService；付款、出貨、退款狀態不可由 Repository 或前端自行判斷。
        return
        [
            new() { Title = "待付款", Items = [WorkItem("訂單付款", "付款結果由金流交易確認。", "Payment", "warning")] },
            new() { Title = "已付款", Items = [] },
            new() { Title = "待出貨", Items = [] },
            new() { Title = "已完成", Items = [] }
        ];
    }

    private static AdminMetricCardViewModel Metric(
        string label,
        string value,
        string note,
        string variant)
    {
        return new AdminMetricCardViewModel
        {
            Label = label,
            Value = value,
            Note = note,
            Variant = variant
        };
    }

    private static AdminWorkItemViewModel WorkItem(
        string title,
        string description,
        string badgeText,
        string variant)
    {
        return new AdminWorkItemViewModel
        {
            Title = title,
            Description = description,
            BadgeText = badgeText,
            BadgeVariant = variant
        };
    }

    private static AdminChartPointViewModel Chart(string label, int percentage, string valueText)
    {
        return new AdminChartPointViewModel
        {
            Label = label,
            Percentage = percentage,
            ValueText = valueText
        };
    }
}
