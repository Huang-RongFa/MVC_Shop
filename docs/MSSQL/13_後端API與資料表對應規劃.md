# 13 後端 API 與資料表對應規劃

## 1. API 規劃目的

此文件用於說明 ASP.NET Core 後端 API 如何對應 MSSQL 電商資料表。建議後端採用分層架構：

```text
Controller
    ↓
Service
    ↓
Repository
    ↓
DbContext / Dapper
    ↓
MSSQL
```

---

## 2. AuthController 登入與驗證

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/auth/login | POST | Users, UserLoginLogs | 使用者登入 |
| /api/auth/logout | POST | UserLoginLogs | 使用者登出 |
| /api/auth/me | GET | Users, UserRoles, Permissions | 查詢目前登入者 |
| /api/auth/refresh-token | POST | Users | 更新 Token |

---

## 3. UsersController 使用者管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/users | GET | Users | 使用者列表 |
| /api/users/{id} | GET | Users | 使用者詳細資料 |
| /api/users | POST | Users | 新增使用者 |
| /api/users/{id} | PUT | Users | 修改使用者 |
| /api/users/{id}/lock | POST | Users | 鎖定帳號 |
| /api/users/{id}/roles | POST | UserRoles | 指派角色 |

---

## 4. RolesController 角色權限管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/roles | GET | Roles | 角色列表 |
| /api/roles | POST | Roles | 新增角色 |
| /api/roles/{id} | PUT | Roles | 修改角色 |
| /api/roles/{id}/permissions | GET | RolePermissions | 查詢角色權限 |
| /api/roles/{id}/permissions | POST | RolePermissions | 設定角色權限 |

---

## 5. ProductsController 商品管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/products | GET | Products, ProductSkus | 商品列表 |
| /api/products/{id} | GET | Products, ProductSkus, ProductImages | 商品詳細 |
| /api/products | POST | Products, ProductSkus | 新增商品 |
| /api/products/{id} | PUT | Products | 修改商品 |
| /api/products/{id}/publish | POST | Products | 商品上架 |
| /api/products/{id}/unpublish | POST | Products | 商品下架 |
| /api/products/{id}/images | POST | ProductImages | 新增商品圖片 |

---

## 6. ProductSkusController SKU 管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/products/{productId}/skus | GET | ProductSkus | 查詢商品 SKU |
| /api/products/{productId}/skus | POST | ProductSkus | 新增 SKU |
| /api/skus/{id} | PUT | ProductSkus | 修改 SKU |
| /api/skus/{id}/price | PATCH | ProductSkus | 修改價格 |
| /api/skus/{id}/attributes | POST | ProductAttributeValues | 設定規格屬性 |

---

## 7. InventoryController 庫存管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/inventory/stocks | GET | InventoryStocks | 查詢庫存 |
| /api/inventory/adjust | POST | InventoryStocks, InventoryTransactions | 調整庫存 |
| /api/inventory/transactions | GET | InventoryTransactions | 庫存異動紀錄 |
| /api/warehouses | GET | Warehouses | 倉庫列表 |
| /api/warehouses | POST | Warehouses | 新增倉庫 |

---

## 8. OrdersController 訂單管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/orders | GET | Orders | 訂單列表 |
| /api/orders/{id} | GET | Orders, OrderItems | 訂單詳細 |
| /api/orders | POST | Orders, OrderItems, InventoryReservations | 建立訂單 |
| /api/orders/{id}/cancel | POST | Orders, OrderStatusHistories, InventoryReservations | 取消訂單 |
| /api/orders/{id}/status | PATCH | Orders, OrderStatusHistories | 修改訂單狀態 |

---

## 9. PaymentsController 付款管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/payments | GET | Payments | 付款列表 |
| /api/orders/{orderId}/payments | POST | Payments | 建立付款 |
| /api/payments/{id}/callback | POST | Payments, PaymentTransactions | 金流回呼 |
| /api/payments/{id} | GET | Payments, PaymentTransactions | 付款詳細 |

---

## 10. ShipmentsController 出貨管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/shipments | GET | Shipments | 出貨列表 |
| /api/orders/{orderId}/shipments | POST | Shipments, ShipmentItems | 建立出貨 |
| /api/shipments/{id}/ship | POST | Shipments, InventoryStocks, InventoryTransactions | 確認出貨 |
| /api/shipments/{id}/delivered | POST | Shipments | 確認送達 |

---

## 11. CouponsController 優惠管理

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/coupons | GET | Coupons | 優惠券列表 |
| /api/coupons | POST | Coupons | 新增優惠券 |
| /api/coupons/{id} | PUT | Coupons | 修改優惠券 |
| /api/coupons/validate | POST | Coupons, CouponUsages | 驗證優惠券 |
| /api/coupons/usages | GET | CouponUsages | 優惠券使用紀錄 |

---

## 12. AuditLogsController 稽核查詢

| API | Method | 對應資料表 | 說明 |
|---|---|---|---|
| /api/audit/admin-actions | GET | AdminActionLogs | 後台操作紀錄 |
| /api/audit/data-changes | GET | AuditLogs | 資料異動紀錄 |
| /api/audit/login-logs | GET | UserLoginLogs | 登入紀錄 |
| /api/audit/errors | GET | SystemErrorLogs | 系統錯誤紀錄 |

---

## 13. Service 層建議

```text
AuthService
UserService
RoleService
ProductService
SkuService
InventoryService
OrderService
PaymentService
ShipmentService
RefundService
CouponService
AuditService
```

---

## 14. Repository 層建議

```text
UserRepository
ProductRepository
InventoryRepository
OrderRepository
PaymentRepository
ShipmentRepository
CouponRepository
AuditRepository
```

---

## 15. 交易控制建議

以下流程一定要使用 Transaction：

1. 建立訂單 + 建立訂單明細 + 保留庫存。
2. 取消訂單 + 釋放庫存 + 更新訂單狀態。
3. 出貨 + 扣庫存 + 更新出貨狀態。
4. 付款成功 + 更新付款狀態 + 更新訂單狀態。
5. 退款成功 + 更新退款狀態 + 更新付款狀態。
6. 使用優惠券 + 更新使用次數 + 建立使用紀錄。

---

## 16. 本章總結

API 規劃應以資料表模組為基礎，但不要讓 Controller 直接操作資料庫。建議將業務流程放在 Service 層，資料存取放在 Repository 層，並在關鍵交易流程中使用 Transaction 保證資料一致性。
