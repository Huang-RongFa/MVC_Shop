# 05｜後台 API / Service / 資料表對應總表

## 結論

後台頁面應只作為入口，正式資料必須由 Admin API 或後台 Service 提供。Controller 負責 HTTP 與授權入口，Service 負責商業規則，Repository 負責資料存取，資料異動必須寫入稽核紀錄。

---

## 1. 命名建議

後台 API 建議統一使用：

```text
/api/admin/{module}
```

例如：

```text
/api/admin/products
/api/admin/orders
/api/admin/refunds
/api/admin/roles
```

避免與前台 API 混在一起。

---

## 2. 對應總表

| 模組 | Razor Route | Policy | 建議 Admin API | Service | 主要資料表 | 稽核 |
|---|---|---|---|---|---|---|
| 登入 | `/admin/login` | AllowAnonymous | `/api/admin/auth/login` | `IAdminAuthService` | Users, UserRoles, Roles, Permissions, UserLoginLogs | LoginLog |
| Dashboard | `/admin` | `Dashboard.Read` | `/api/admin/dashboard` | `IAdminDashboardService` | Orders, Payments, InventoryStocks, Refunds, AuditLogs | 只讀 |
| Analytics | `/admin/analytics` | `Analytics.Read` | `/api/admin/analytics` | `IAdminAnalyticsService` | Orders, Payments, Refunds, OrderItems, CouponUsages | 只讀 |
| Users | `/admin/users` | `Users.Manage` | `/api/admin/users` | `IAdminUserService` | Users, UserRoles, UserLoginLogs | AdminActionLog, AuditLog |
| Roles | `/admin/roles` | `Roles.Manage` | `/api/admin/roles` | `IAdminRoleService` | Roles, Permissions, RolePermissions | AdminActionLog, AuditLog |
| Products | `/admin/products` | `Products.Read` | `/api/admin/products` | `IAdminProductService` | Products, ProductCategories, ProductImages | AuditLog |
| Product Edit | `/admin/products/edit` | `Products.Write` | `/api/admin/products/{id}` | `IAdminProductService` | Products, ProductImages | AdminActionLog, AuditLog |
| Product SKUs | `/admin/products/skus` | `Products.Write` | `/api/admin/products/{id}/skus` | `IAdminSkuService` | ProductSkus, ProductAttributeValues | AuditLog |
| Inventory | `/admin/inventory` | `Inventory.Read` | `/api/admin/inventory` | `IAdminInventoryService` | InventoryStocks, Warehouses, InventoryTransactions | AuditLog |
| Inventory Adjust | `/admin/inventory` | `Inventory.Adjust` | `/api/admin/inventory/adjustments` | `IAdminInventoryService` | InventoryStocks, InventoryTransactions | AdminActionLog, AuditLog |
| Orders | `/admin/orders` | `Orders.Read` | `/api/admin/orders` | `IAdminOrderService` | Orders, OrderItems, OrderStatusHistories | AuditLog |
| Order Details | `/admin/orders/details` | `Orders.Read` | `/api/admin/orders/{id}` | `IAdminOrderService` | Orders, OrderItems, Payments, Shipments, Refunds | 只讀 / AuditLog |
| Payments | `/admin/payments` | `Payments.Read` | `/api/admin/payments` | `IAdminPaymentService` | Payments, PaymentTransactions | 只讀 / AuditLog |
| Shipments | `/admin/shipments` | `Shipments.Manage` | `/api/admin/shipments` | `IAdminShipmentService` | Shipments, ShipmentItems, InventoryReservations | AdminActionLog, AuditLog |
| Refunds | `/admin/refunds` | `Refunds.Manage` | `/api/admin/refunds` | `IAdminRefundService` | Refunds, RefundItems, Payments, Orders | AdminActionLog, AuditLog |
| Coupons | `/admin/coupons` | `Coupons.Manage` | `/api/admin/coupons` | `IAdminCouponService` | Coupons, CouponUsages | AdminActionLog, AuditLog |
| Promotions | `/admin/promotions` | `Promotions.Manage` | `/api/admin/promotions` | `IAdminPromotionService` | Promotions, PromotionProducts | AdminActionLog, AuditLog |
| Inbox | `/admin/inbox` | `Messages.Read` | `/api/admin/notifications` | `IAdminNotificationService` | 系統通知表或由規則產生 | AdminActionLog |
| Audit Logs | `/admin/audit-logs` | `AuditLogs.Read` | `/api/admin/audit-logs` | `IAdminAuditLogService` | AuditLogs, AdminActionLogs, SystemErrorLogs | 只讀 |
| Settings | `/admin/settings` | `Settings.Manage` | `/api/admin/settings` | `IAdminSettingsService` | 系統設定表 | AdminActionLog, AuditLog |

---

## 3. Controller 責任

後台 API Controller 應只做：

```text
□ Route
□ HTTP Method
□ DTO Binding
□ ModelState 檢查
□ Authorize Policy
□ 呼叫 Service
□ 回傳 ApiResponse
```

不應做：

```text
□ 直接操作 DbContext
□ 直接回傳 Entity
□ 寫商業規則
□ 自己計算折扣
□ 自己判斷庫存流程
□ 自己改訂單狀態
```

---

## 4. Service 責任

Service 應做：

```text
□ 權限檢查
□ 狀態轉換檢查
□ 交易一致性處理
□ DTO 組裝
□ 重要操作寫入稽核
□ 呼叫 Repository
```

Service 方法命名範例：

```csharp
Task<PagedResult<AdminProductListItemDto>> SearchProductsAsync(AdminProductSearchRequest request, ClaimsPrincipal user, CancellationToken ct);
Task<AdminProductDetailDto> GetProductAsync(long productId, ClaimsPrincipal user, CancellationToken ct);
Task<long> CreateProductAsync(AdminProductCreateRequest request, ClaimsPrincipal user, CancellationToken ct);
Task PublishProductAsync(long productId, ClaimsPrincipal user, CancellationToken ct);
Task AdjustInventoryAsync(AdminInventoryAdjustmentRequest request, ClaimsPrincipal user, CancellationToken ct);
Task ApproveRefundAsync(long refundId, AdminRefundApproveRequest request, ClaimsPrincipal user, CancellationToken ct);
```

---

## 5. DTO 分層

建議資料夾：

```text
Application/DTOs/Admin/
├─ Auth/
├─ Dashboard/
├─ Users/
├─ Roles/
├─ Products/
├─ Inventory/
├─ Orders/
├─ Payments/
├─ Marketing/
├─ Notifications/
├─ AuditLogs/
└─ Settings/
```

DTO 原則：

```text
□ Admin DTO 與 Storefront DTO 分開
□ 不回傳 PasswordHash、PasswordSalt
□ 不把 CostPrice 回傳給無權限角色
□ 不把 Provider Payload 原文回傳給一般人員
□ Request DTO 與 Response DTO 分開
□ 列表 DTO 與詳細 DTO 分開
```

---

## 6. API 規格範例

### 商品列表

```http
GET /api/admin/products?keyword=&status=&categoryId=&page=1&pageSize=20
Policy: Products.Read
```

回傳：

```json
{
  "succeeded": true,
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 20,
    "totalCount": 0
  }
}
```

### 庫存調整

```http
POST /api/admin/inventory/adjustments
Policy: Inventory.Adjust
CSRF: required
```

Request：

```json
{
  "warehouseId": 1,
  "skuId": 10001,
  "quantity": 5,
  "reason": "盤點調整",
  "remark": "2026-06 月盤點"
}
```

必要行為：

```text
□ 檢查 Inventory.Adjust
□ 建立 InventoryTransaction
□ 更新 InventoryStock
□ 寫入 AuditLog
□ 交易失敗需 rollback
```

### 退款核准

```http
POST /api/admin/refunds/{refundId}/approve
Policy: Refunds.Manage
CSRF: required
```

必要行為：

```text
□ 檢查 Refunds.Manage
□ 檢查可退款金額
□ 檢查退款狀態是否 Requested
□ 建立或更新 Refund / RefundItems
□ 呼叫金流退款流程或標記待金流處理
□ 寫入 AdminActionLog 與 AuditLog
```

---

## 7. 交易一致性要求

下列流程必須使用資料庫交易：

```text
□ 建立訂單 + 建立訂單明細 + 建立庫存保留
□ 付款成功 + 更新付款狀態 + 更新訂單付款狀態
□ 出貨確認 + 建立出貨明細 + 消耗保留庫存 + 更新訂單出貨狀態
□ 建立退款 + 退款品項 + 更新付款 / 訂單狀態
□ 調整庫存 + 建立庫存異動紀錄
□ 指派角色 + 寫入操作紀錄
```

若使用 SQL Server retrying execution strategy，交易需依 EF Core 建議包在 execution strategy 中執行，避免手動交易與重試策略衝突。

---

## 8. 驗收條件

```text
□ 所有後台 API 皆位於 /api/admin/*
□ 所有後台 API 都有明確 Policy
□ Controller 不直接操作 DbContext
□ Response 不直接回傳 Entity
□ 高風險 API 有 CSRF 驗證
□ 高風險操作寫入 AdminActionLog / AuditLog
□ 交易流程失敗會 rollback
□ 前台 API 與後台 API 邊界清楚
```
