# 06｜RESTful API、前台 API 與後台 API 設計

## 1. 本章目的

本章規劃前台 API 與後台 API。即使兩者操作同一套 MSSQL，也必須拆成不同邊界、不同 Controller、不同 DTO 與不同權限規則。

```text
前台 API：給 Storefront App 使用
後台 API：給 Admin App 使用
```

正式部署時，API 設計還要加入：

```text
權限檢查
資料擁有者檢查
分頁限制
錯誤格式
冪等處理
Rate Limiting
交易一致性
稽核紀錄
```

---

## 2. RESTful 命名原則

建議使用名詞與資源階層：

```text
GET    /api/storefront/products
GET    /api/storefront/products/{id}
POST   /api/storefront/cart/items
POST   /api/storefront/orders

GET    /api/admin/products
POST   /api/admin/products
PUT    /api/admin/products/{id}
POST   /api/admin/products/{id}/publish
```

不建議：

```text
/api/getProduct
/api/createProduct
/api/deleteProduct
```

---

## 3. Controller 分界

| API 邊界 | Controller 範例 | 負責功能 |
|---|---|---|
| 前台 API | StorefrontProductsController | 顧客瀏覽商品 |
| 前台 API | StorefrontCartController | 顧客購物車 |
| 前台 API | StorefrontOrdersController | 顧客建立與查詢自己的訂單 |
| 前台 API | StorefrontPaymentsController | 顧客建立付款 |
| 後台 API | AdminProductsController | 商品管理 |
| 後台 API | AdminInventoryController | 庫存管理 |
| 後台 API | AdminOrdersController | 訂單管理 |
| 後台 API | AdminPaymentsController | 付款管理 |
| 後台 API | AdminShipmentsController | 出貨管理 |
| 後台 API | AdminRefundsController | 退款管理 |
| 後台 API | AdminCouponsController | 優惠管理 |
| 後台 API | AdminAuditLogsController | 稽核查詢 |

---

## 4. 前台 API 規劃

| API | Method | 對應資料表 | 說明 | 安全要求 |
|---|---|---|---|---|
| `/api/storefront/auth/login` | POST | Users, UserLoginLogs | 前台登入 | Rate Limit、登入紀錄 |
| `/api/storefront/auth/logout` | POST | UserLoginLogs | 前台登出 | CSRF |
| `/api/storefront/auth/me` | GET | Users | 目前前台會員 | Cookie 驗證 |
| `/api/storefront/products` | GET | Products, ProductSkus | 前台商品列表 | 分頁、只顯示可販售商品 |
| `/api/storefront/products/{id}` | GET | Products, ProductSkus, ProductImages | 商品詳細 | 不顯示未上架商品 |
| `/api/storefront/cart` | GET | ShoppingCarts, ShoppingCartItems | 查詢自己的購物車 | 本人資料檢查 |
| `/api/storefront/cart/items` | POST | ShoppingCartItems | 加入購物車 | CSRF、商品與庫存檢查 |
| `/api/storefront/orders` | POST | Orders, OrderItems, InventoryReservations | 建立訂單並保留庫存 | CSRF、Transaction、價格重算 |
| `/api/storefront/me/orders` | GET | Orders | 查詢自己的訂單 | 本人資料檢查 |
| `/api/storefront/me/orders/{id}` | GET | Orders, OrderItems | 查詢自己的訂單詳細 | 本人資料檢查 |
| `/api/storefront/orders/{id}/payments` | POST | Payments | 建立付款 | 本人訂單檢查、CSRF |
| `/api/storefront/coupons/validate` | POST | Coupons, CouponUsages | 驗證優惠券 | 不回傳內部規則細節 |

---

## 5. 後台 API 規劃

| API | Method | 對應資料表 | 說明 | 安全要求 |
|---|---|---|---|---|
| `/api/admin/auth/login` | POST | Users, UserLoginLogs | 後台登入 | 嚴格 Rate Limit、登入紀錄、MFA |
| `/api/admin/auth/me` | GET | Users, UserRoles, Permissions | 目前後台使用者 | Cookie 驗證 |
| `/api/admin/users` | GET | Users | 使用者列表 | Users.Read |
| `/api/admin/users/{id}/roles` | POST | UserRoles | 指派角色 | Roles.Manage、稽核 |
| `/api/admin/roles/{id}/permissions` | POST | RolePermissions | 設定角色權限 | Roles.Manage、稽核 |
| `/api/admin/products` | GET | Products, ProductSkus | 商品列表 | Products.Read |
| `/api/admin/products` | POST | Products, ProductSkus | 新增商品 | Products.Write、CSRF、稽核 |
| `/api/admin/products/{id}` | PUT | Products | 修改商品 | Products.Write、稽核 |
| `/api/admin/products/{id}/publish` | POST | Products | 商品上架 | Products.Publish、稽核 |
| `/api/admin/inventory/stocks` | GET | InventoryStocks | 查詢庫存 | Inventory.Read |
| `/api/admin/inventory/adjust` | POST | InventoryStocks, InventoryTransactions | 調整庫存 | Inventory.Adjust、Transaction、稽核 |
| `/api/admin/orders` | GET | Orders | 訂單列表 | Orders.Read |
| `/api/admin/orders/{id}` | GET | Orders, OrderItems | 訂單詳細 | Orders.Read |
| `/api/admin/orders/{id}/status` | PATCH | Orders, OrderStatusHistories | 修改訂單狀態 | Orders.Manage、狀態機、稽核 |
| `/api/admin/payments` | GET | Payments | 付款列表 | Payments.Read |
| `/api/admin/payments/{id}/callback` | POST | Payments, PaymentTransactions | 金流回呼 | 驗簽、冪等、Transaction |
| `/api/admin/shipments` | GET | Shipments | 出貨列表 | Shipments.Read |
| `/api/admin/orders/{orderId}/shipments` | POST | Shipments, ShipmentItems | 建立出貨 | Shipments.Manage、Transaction |
| `/api/admin/shipments/{id}/ship` | POST | Shipments, InventoryStocks, InventoryTransactions | 確認出貨並扣庫 | Shipments.Manage、Transaction、稽核 |
| `/api/admin/refunds` | GET | Refunds | 退款列表 | Refunds.Read |
| `/api/admin/orders/{orderId}/refunds` | POST | Refunds, RefundItems | 整筆或部分退款 | Refunds.Manage、不可超額、Transaction |
| `/api/admin/coupons` | GET | Coupons | 優惠券列表 | Coupons.Read |
| `/api/admin/promotions` | GET | Promotions | 促銷列表 | Promotions.Read |
| `/api/admin/audit/admin-actions` | GET | AdminActionLogs | 後台操作紀錄 | AuditLogs.Read |

---

## 6. 重要交易 API 規則

| 流程 | 規則 |
|---|---|
| 建立訂單 | 從資料庫重算價格，下單時建立訂單、明細、保留庫存與折扣紀錄 |
| 付款成功 | 訂單進入 Paid，不自動出貨 |
| 建立出貨 | 已付款訂單才可進入出貨流程 |
| 確認出貨 | 出貨確認時才正式扣實際庫存 |
| 退款 | 支援整筆退款與部分品項退款，不可超過可退款金額 |
| 優惠 | 優惠券與促銷可疊加，但要寫入 OrderDiscounts |
| 金流回呼 | 必須驗簽、檢查交易編號、冪等處理 |

---

## 7. API Response 與錯誤格式

成功回應可以統一：

```json
{
  "success": true,
  "message": "操作成功",
  "data": {}
}
```

分頁回應：

```json
{
  "items": [],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20
}
```

錯誤回應建議採用 ProblemDetails 或一致格式：

```json
{
  "type": "https://example.com/errors/validation-error",
  "title": "請求資料驗證失敗",
  "status": 400,
  "detail": "部分欄位不符合規則",
  "errors": {
    "name": ["商品名稱為必填"]
  }
}
```

正式環境錯誤不得包含：

```text
Stack Trace
SQL 語法
Connection String
檔案路徑
內部類別名稱
金流密鑰
```

---

## 8. HTTP Status Code 建議

| 狀態碼 | 意義 | 範例 |
|---|---|---|
| 200 | 成功 | 查詢成功 |
| 201 | 新增成功 | 建立商品、訂單成功 |
| 204 | 成功但無內容 | 刪除或登出成功 |
| 400 | 請求格式錯誤 | 欄位驗證失敗 |
| 401 | 未登入 | Cookie 無效或過期 |
| 403 | 無權限 | 已登入但沒有 Permission |
| 404 | 找不到資料 | 商品或訂單不存在 |
| 409 | 資料衝突 | 重複付款、庫存不足、狀態不允許 |
| 429 | 請求過多 | Rate Limit |
| 500 | 伺服器錯誤 | 未預期錯誤 |

---

## 9. API 防濫用與效能限制

```text
□ 列表 API 必須分頁
□ pageSize 設最大值，例如 100
□ 搜尋與排序欄位使用白名單
□ 後台報表限制日期區間
□ Login、搜尋、報表、金流回呼設 Rate Limit
□ 大量匯出不直接同步產生，改背景工作或限制筆數
□ 圖片上傳限制大小、類型、掃描與儲存位置
```

---

## 10. RESTful API 檢查清單

```text
□ 前台 API 與後台 API 分開
□ API 使用名詞而非動詞
□ 使用 HTTP Method 表達操作
□ 使用正確 HTTP Status Code
□ 不直接回傳 Entity
□ 前台 API 檢查資料擁有者
□ 後台 API 檢查 Role / Permission
□ 使用 Request / Response DTO
□ 錯誤訊息格式統一
□ 大量資料使用分頁
□ 寫入型 API 使用 CSRF 防護
□ 跨多張資料表流程使用 Transaction
□ 金流與退款流程具備冪等處理
□ Swagger 僅供開發或受保護環境使用
```

---

## 11. 本章總結

API 設計應以資料表模組為基礎，但不等於直接開 CRUD。前台 API 服務顧客購物流程，後台 API 服務管理流程；兩者可共用 Service 或資料表，但不應共用同一套 Controller 與 DTO。正式上線前，必須補上權限、CSRF、交易、冪等、Rate Limiting 與錯誤處理。
