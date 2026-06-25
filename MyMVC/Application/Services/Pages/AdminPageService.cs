using System.Security.Claims;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Application.Security;
using MyWeb.Models.ViewModels.Admin;

namespace MyWeb.Application.Services.Pages;

/// <summary>
/// 組裝後台 Razor ViewModel。
/// 目前用於呈現管理頁骨架；正式環境應由各模組 Service 提供資料並在 Service 層檢查權限。
/// </summary>
public sealed class AdminPageService : IAdminPageService
{
    private static readonly IReadOnlyList<AdminModuleDefinition> ModuleDefinitions =
    [
        new("analytics", "數據分析", "Analytics", "Analytics.Read", "查看營收、付款成功率、退款率與會員成長。"),
        new("users", "使用者管理", "Users", "Users.Manage", "管理前台會員、後台員工與帳號狀態。"),
        new("roles", "角色權限", "Roles", "Roles.Manage", "維護角色、權限與後台選單可見範圍。"),
        new("products", "商品管理", "Products", "Products.Read", "維護商品主檔、上下架狀態與分類。"),
        new("product-edit", "商品新增修改", "ProductEdit", "Products.Write", "新增或修改商品資料。"),
        new("product-skus", "SKU 管理", "ProductSkus", "Products.Write", "維護商品規格、售價與可售狀態。"),
        new("inventory", "庫存管理", "Inventory", "Inventory.Read", "查詢庫存、保留庫存與庫存異動。"),
        new("orders", "訂單管理", "Orders", "Orders.Read", "查詢訂單狀態、付款狀態與出貨狀態。"),
        new("order-details", "訂單詳細", "OrderDetails", "Orders.Read", "檢視訂單主檔、明細、付款、出貨與退款。"),
        new("payments", "付款管理", "Payments", "Payments.Read", "查詢付款紀錄與金流交易狀態。"),
        new("shipments", "出貨管理", "Shipments", "Shipments.Manage", "管理出貨流程與物流狀態。"),
        new("refunds", "退款管理", "Refunds", "Refunds.Manage", "處理整筆與部分退款。"),
        new("coupons", "優惠券", "Coupons", "Coupons.Manage", "維護優惠券規則與使用限制。"),
        new("promotions", "促銷活動", "Promotions", "Promotions.Manage", "維護促銷活動與商品範圍。"),
        new("inbox", "訊息通知", "Inbox", "Messages.Read", "查看客服訊息、系統通知與營運警示。"),
        new("audit-logs", "稽核紀錄", "AuditLogs", "AuditLogs.Read", "查詢後台重要操作與資料異動紀錄。"),
        new("settings", "系統設定", "Settings", "Settings.Manage", "管理後台偏好、通知設定與安全設定。")
    ];

    public Task<AdminDashboardViewModel> GetDashboardAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsurePermission(user, AdminPermissionCodes.DashboardRead);

        var canReadDashboard = true;
        var quickLinks = ModuleDefinitions
            .Where(module => HasPermission(user, module.PermissionName))
            .Where(module => module.Key != "product-edit" && module.Key != "order-details")
            .Take(6)
            .Select(module => new AdminQuickLinkViewModel
            {
                Title = module.Title,
                Description = module.Description,
                Controller = "Admin",
                Action = module.Action,
                PermissionName = module.PermissionName
            })
            .ToArray();

        return Task.FromResult(new AdminDashboardViewModel
        {
            DisplayName = user.Identity?.Name ?? "管理員",
            RoleDescriptions = GetRoleDescriptions(user),
            QuickLinks = quickLinks,
            Metrics = canReadDashboard
                ?
                [
                    Metric("今日營收", "待串接", "資料來源：Orders / Payments", "neutral"),
                    Metric("今日訂單", "待串接", "資料來源：Orders", "neutral"),
                    Metric("待出貨", "待串接", "資料來源：Shipments", "warning"),
                    Metric("低庫存 SKU", "待串接", "資料來源：InventoryStocks", "danger")
                ]
                : [],
            WorkItems = BuildRoleAwareWorkItems(user),
            RecentOrders =
                HasPermission(user, "Orders.Read")
                    ?
                    [
                        Row("🧾", ["FM-10428", "林佳玲", "NT$1,280", "待出貨"], "待處理", "warning", "查看"),
                        Row("💳", ["FM-10427", "王柏翰", "NT$860", "付款確認"], "已付款", "success", "對帳"),
                        Row("↩", ["FM-10421", "陳怡君", "NT$540", "退款審核"], "需審核", "danger", "處理")
                    ]
                    : [],
            RecentAuditEvents =
                HasPermission(user, "AuditLogs.Read")
                    ?
                    [
                        Audit("庫存調整待稽核", "草莓禮盒保留庫存需比對出貨單。", "5 分鐘前", "warning"),
                        Audit("後台登入", "管理員登入成功，已由 Cookie Authentication 建立狀態。", "18 分鐘前", "success"),
                        Audit("促銷草稿", "週末鮮果組合尚未發布，正式發布需記錄操作者。", "42 分鐘前", "neutral")
                    ]
                    : [],
            SystemAlerts = BuildPermissionSummary(user, quickLinks.Length)
        });
    }

    public Task<AdminAnalyticsViewModel> GetAnalyticsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsurePermission(user, "Analytics.Read");

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
        cancellationToken.ThrowIfCancellationRequested();

        // 目前 moduleKey 決定頁面骨架；正式功能接上專責 Service 後仍需保留這層 Permission 檢查。
        var viewModel = moduleKey switch
        {
            "users" => Module(
                "使用者管理",
                "管理前台會員、後台員工與帳號狀態。",
                "Users.Manage",
                "新增使用者",
                ["姓名", "Email", "身分", "狀態"],
                [Metric("會員數", "待串接", "Users", "neutral"), Metric("停用帳號", "待串接", "Users", "warning")],
                [Row("👤", ["林佳玲", "customer@example.com", "Member", "啟用"], "可購物", "success", "檢視")]),
            "roles" => Module(
                "角色權限管理",
                "維護角色、權限與後台選單可見範圍。",
                "Roles.Manage",
                "新增角色",
                ["角色", "權限數", "使用者數", "狀態"],
                [Metric("角色數", "待串接", "Roles", "neutral"), Metric("權限數", "待串接", "Permissions", "neutral")],
                [Row("🛡", ["Admin", "全權限", "待串接", "受控"], "高權限", "danger", "管理")]),
            "products" => Module(
                "商品管理",
                "維護商品主檔、上下架狀態、分類與商品圖片。",
                "Products.Read",
                "新增商品",
                ["商品", "分類", "SKU", "狀態"],
                [Metric("已上架", "待串接", "Products", "success"), Metric("草稿", "待串接", "Products", "neutral")],
                [
                    Row("🥭", ["玉井愛文芒果", "當季水果", "3", "已上架"], "Active", "success", "編輯"),
                    Row("🥦", ["西螺有機花椰菜", "有機蔬菜", "2", "待補圖"], "Draft", "warning", "檢查")
                ],
                ProductWorkflow()),
            "product-edit" => Module(
                "商品新增修改",
                "新增或修改商品資料，正式儲存時需寫入後台操作紀錄。",
                "Products.Write",
                "儲存商品",
                ["欄位", "狀態", "資料來源", "備註"],
                [Metric("編輯模式", "待串接", "ProductEdit", "neutral")],
                [Row("✏️", ["基本資料", "待驗證", "ProductService", "不可由 Controller 直接寫入 DbContext"], "Draft", "neutral", "編輯")]),
            "product-skus" => Module(
                "SKU 管理",
                "維護商品規格、售價、可售狀態與庫存對應。",
                "Products.Write",
                "新增 SKU",
                ["SKU", "規格", "售價", "狀態"],
                [Metric("SKU 數", "待串接", "ProductSkus", "neutral")],
                [Row("🏷", ["MANGO-BOX-2KG", "2 公斤禮盒", "NT$880", "可售"], "Active", "success", "檢視")]),
            "inventory" => Module(
                "庫存管理",
                "查詢庫存、保留庫存與庫存異動；調整庫存需有稽核紀錄。",
                "Inventory.Read",
                "調整庫存",
                ["SKU", "倉庫", "可售", "保留"],
                [Metric("低庫存", "待串接", "InventoryStocks", "danger"), Metric("保留庫存", "待串接", "InventoryReservations", "warning")],
                [
                    Row("📦", ["MANGO-BOX-2KG", "台南常溫倉", "18", "4"], "安全", "success", "檢視"),
                    Row("⚠", ["STRAWBERRY-BOX", "苗栗冷藏倉", "3", "2"], "低庫存", "danger", "補貨")
                ]),
            "orders" => Module(
                "訂單管理",
                "查詢訂單狀態、付款狀態與出貨狀態。",
                "Orders.Read",
                "更新狀態",
                ["訂單編號", "會員", "金額", "狀態"],
                [Metric("待付款", "待串接", "Orders", "warning"), Metric("已付款", "待串接", "Orders", "success")],
                [
                    Row("🧾", ["FM-10428", "林佳玲", "NT$1,280", "待出貨"], "Paid", "warning", "查看"),
                    Row("🚚", ["FM-10420", "王柏翰", "NT$2,140", "已出貨"], "Shipped", "success", "追蹤")
                ],
                OrderWorkflow()),
            "order-details" => Module(
                "訂單詳細",
                "顯示訂單主檔、明細、付款、出貨、退款與狀態歷史。",
                "Orders.Read",
                "更新訂單",
                ["區塊", "資料來源", "狀態", "備註"],
                [Metric("訂單狀態", "待串接", "Orders", "neutral")],
                [Row("🧩", ["付款紀錄", "Payments", "待查詢", "Callback 必須驗簽與冪等"], "Review", "warning", "檢視")]),
            "payments" => Module(
                "付款管理",
                "查詢付款紀錄與金流交易狀態，Callback 必須驗簽且具備冪等處理。",
                "Payments.Read",
                "查看交易",
                ["付款單", "訂單", "金額", "狀態"],
                [Metric("付款成功", "待串接", "Payments", "success"), Metric("付款失敗", "待串接", "PaymentTransactions", "danger")],
                [Row("💳", ["PAY-90018", "FM-10427", "NT$860", "授權成功"], "Success", "success", "對帳")]),
            "shipments" => Module(
                "出貨管理",
                "管理出貨流程；出貨確認後才扣實際庫存。",
                "Shipments.Manage",
                "建立出貨",
                ["出貨單", "訂單", "物流", "狀態"],
                [Metric("待出貨", "待串接", "Shipments", "warning"), Metric("已出貨", "待串接", "Shipments", "success")],
                [Row("🚚", ["SHIP-22018", "FM-10428", "冷藏宅配", "待撿貨"], "Pending", "warning", "處理")]),
            "refunds" => Module(
                "退款管理",
                "處理整筆退款與部分退款，所有操作需寫入稽核紀錄。",
                "Refunds.Manage",
                "建立退款",
                ["退款單", "訂單", "金額", "狀態"],
                [Metric("待審核", "待串接", "Refunds", "warning"), Metric("已退款", "待串接", "Refunds", "success")],
                [Row("↩", ["REF-31004", "FM-10421", "NT$540", "審核中"], "Review", "warning", "審核")]),
            "coupons" => Module(
                "優惠券管理",
                "維護優惠券規則、使用限制與使用紀錄。",
                "Coupons.Manage",
                "新增優惠券",
                ["優惠券", "折扣", "期間", "狀態"],
                [Metric("有效優惠券", "待串接", "Coupons", "success")],
                [Row("🎟", ["FRESH100", "滿千折百", "週末限定", "草稿"], "Draft", "neutral", "編輯")]),
            "promotions" => Module(
                "促銷管理",
                "維護促銷活動與商品範圍；折扣結果需記錄來源。",
                "Promotions.Manage",
                "新增促銷",
                ["促銷", "商品範圍", "期間", "狀態"],
                [Metric("進行中", "待串接", "Promotions", "success")],
                [Row("🔥", ["夏季鮮果節", "水果分類", "6/20 - 6/30", "待發布"], "Review", "warning", "檢查")]),
            "inbox" => Module(
                "訊息 / 通知",
                "集中查看客服訊息、系統通知與營運警示。",
                "Messages.Read",
                "建立通知",
                ["來源", "主旨", "時間", "狀態"],
                [Metric("未讀訊息", "待串接", "Messages", "warning")],
                [Row("✉️", ["客服", "冷藏配送時間調整", "10 分鐘前", "未讀"], "New", "warning", "回覆")]),
            "audit-logs" => Module(
                "稽核紀錄",
                "查詢後台重要操作與資料異動紀錄。",
                "AuditLogs.Read",
                "匯出紀錄",
                ["時間", "操作者", "動作", "結果"],
                [Metric("今日操作", "待串接", "AdminActionLogs", "neutral")],
                [Row("🔍", ["10:18", "admin@example.com", "庫存調整草稿", "待確認"], "Tracked", "neutral", "檢視")]),
            "settings" => Module(
                "系統設定",
                "管理後台偏好、通知設定與安全設定。",
                "Settings.Manage",
                "儲存設定",
                ["設定", "目前值", "風險", "狀態"],
                [Metric("安全設定", "待串接", "Settings", "neutral")],
                [Row("⚙", ["Cookie SameSite", "Lax / Strict 規劃", "中", "待確認"], "Review", "warning", "檢查")]),
            _ => Module(
                "後台管理",
                "此頁尚未定義模組。",
                "Dashboard.Read",
                string.Empty,
                ["項目", "資料來源", "狀態", "備註"],
                [],
                [])
        };

        EnsurePermission(user, viewModel.PermissionName);
        return Task.FromResult(viewModel);
    }

    private static AdminModulePageViewModel Module(
        string title,
        string description,
        string permission,
        string primaryActionText,
        IReadOnlyList<string> columns,
        IReadOnlyList<AdminMetricCardViewModel> metrics,
        IReadOnlyList<AdminTableRowViewModel> rows,
        IReadOnlyList<AdminKanbanColumnViewModel>? workflowColumns = null)
    {
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
            Rows = rows,
            EmptyStateTitle = "尚未串接正式資料",
            EmptyStateDescription = "此頁已完成 ViewModel 與版面骨架，後續可替換為正式 Service 查詢。"
        };
    }

    private static IReadOnlyList<AdminRoleDescriptionViewModel> GetRoleDescriptions(ClaimsPrincipal user)
    {
        var roleDescriptions = user.FindAll(AdminClaimTypes.RoleInfo)
            .Select(claim =>
            {
                var parts = claim.Value.Split(AdminClaimTypes.RoleInfoSeparator);
                return new AdminRoleDescriptionViewModel
                {
                    RoleCode = parts.Length > 0 ? parts[0] : string.Empty,
                    RoleName = parts.Length > 1 ? parts[1] : string.Empty,
                    Description = parts.Length > 2 ? parts[2] : string.Empty
                };
            })
            .Where(role => !string.IsNullOrWhiteSpace(role.RoleCode))
            .GroupBy(role => role.RoleCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(role => role.RoleName)
            .ToArray();

        if (roleDescriptions.Length > 0)
        {
            return roleDescriptions;
        }

        return user.FindAll(ClaimTypes.Role)
            .Select(claim => new AdminRoleDescriptionViewModel
            {
                RoleCode = claim.Value,
                RoleName = claim.Value,
                Description = "此角色尚未提供說明。"
            })
            .GroupBy(role => role.RoleCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(role => role.RoleName)
            .ToArray();
    }

    private static IReadOnlyList<AdminWorkItemViewModel> BuildRoleAwareWorkItems(ClaimsPrincipal user)
    {
        var items = new List<AdminWorkItemViewModel>();

        if (HasPermission(user, "Orders.Read"))
        {
            items.Add(WorkItem("確認付款完成訂單", "付款成功只代表 Paid，不等於自動出貨。", "Orders", "warning"));
        }

        if (HasPermission(user, "Inventory.Read"))
        {
            items.Add(WorkItem("檢查低庫存商品", "出貨確認後才扣實際庫存，調整需留下紀錄。", "Inventory", "danger"));
        }

        if (HasPermission(user, "Refunds.Manage"))
        {
            items.Add(WorkItem("審核退款申請", "退款支援整筆與部分退款，需保留稽核紀錄。", "Refunds", "warning"));
        }

        if (HasPermission(user, "Products.Read"))
        {
            items.Add(WorkItem("維護商品資料", "商品上下架、SKU 與價格仍需由後端 Service 驗證。", "Products", "success"));
        }

        if (HasPermission(user, "Coupons.Manage") || HasPermission(user, "Promotions.Manage"))
        {
            items.Add(WorkItem("檢查行銷活動", "優惠券與促銷可疊加，折扣來源需完整記錄。", "Marketing", "neutral"));
        }

        if (items.Count == 0)
        {
            items.Add(WorkItem("尚未授權管理頁面", "請由系統管理員指派對應角色或權限後再操作。", "No Access", "danger"));
        }

        return items;
    }

    private static IReadOnlyList<AdminWorkItemViewModel> BuildPermissionSummary(
        ClaimsPrincipal user,
        int quickLinkCount)
    {
        var permissionCount = user.Claims
            .Where(claim => string.Equals(claim.Type, AdminClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        return
        [
            WorkItem("可用管理頁面", $"目前選單會顯示 {quickLinkCount} 個可進入頁面。", "Menu", "success"),
            WorkItem("權限來源", $"Cookie Claims 內含 {permissionCount} 個 Permission，頁面與 Service 依此檢查。", "Permission", "neutral")
        ];
    }

    private static void EnsurePermission(ClaimsPrincipal user, string permission)
    {
        if (!HasPermission(user, permission))
        {
            throw new UnauthorizedAccessException($"Missing admin permission: {permission}");
        }
    }

    private static bool HasPermission(ClaimsPrincipal user, string permission)
    {
        return user.Claims.Any(claim =>
            string.Equals(claim.Type, AdminClaimTypes.Permission, StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<AdminKanbanColumnViewModel> ProductWorkflow()
    {
        // TODO: 商品流程需接 ProductService，依草稿、審核、上架、下架狀態查詢正式資料。
        return
        [
            new() { Title = "草稿", Items = [WorkItem("商品草稿", "等待 Products.Write 串接。", "Draft", "neutral")] },
            new() { Title = "上架", Items = [WorkItem("玉井愛文芒果", "前台可瀏覽，價格仍由後端確認。", "Active", "success")] },
            new() { Title = "下架", Items = [] },
            new() { Title = "封存", Items = [] }
        ];
    }

    private static IReadOnlyList<AdminKanbanColumnViewModel> OrderWorkflow()
    {
        // TODO: 訂單流程需接 OrderService；付款、出貨、退款狀態不可由 Repository 或前端自行判斷。
        return
        [
            new() { Title = "待付款", Items = [WorkItem("付款逾時檢查", "需由 OrderService 判斷狀態轉換。", "Payment", "warning")] },
            new() { Title = "已付款", Items = [WorkItem("FM-10428", "等待建立出貨單。", "Paid", "success")] },
            new() { Title = "待出貨", Items = [WorkItem("冷藏宅配撿貨", "出貨確認後才扣實際庫存。", "Ship", "warning")] },
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

    private static AdminTableRowViewModel Row(
        string leadingIcon,
        IReadOnlyList<string> cells,
        string statusText,
        string variant,
        string actionText)
    {
        return new AdminTableRowViewModel
        {
            LeadingIcon = leadingIcon,
            Cells = cells,
            StatusText = statusText,
            StatusVariant = variant,
            ActionText = actionText
        };
    }

    private static AdminAuditEventViewModel Audit(
        string actionName,
        string description,
        string occurredAtText,
        string variant)
    {
        return new AdminAuditEventViewModel
        {
            ActionName = actionName,
            Description = description,
            OccurredAtText = occurredAtText,
            Variant = variant
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

    private sealed record AdminModuleDefinition(
        string Key,
        string Title,
        string Action,
        string PermissionName,
        string Description);
}
