# 02｜後端分層、Repository 與 Service

## 1. 本章目的

本章定義後端分層規則。正式電商系統不可讓 Controller 直接操作 DbContext，也不可把訂單、付款、出貨、退款當成單純 CRUD。

基本分層：

```text
Controller：API 入口
Service：商業流程、規則、權限、交易
Repository：資料查詢與儲存
DbContext：EF Core 資料庫入口
Entity：資料庫模型
DTO：API 輸入輸出模型
```

---

## 2. 分層責任

| 層級 | 負責內容 | 不應該做的事 |
|---|---|---|
| Controller | 接收 Request、ModelState 驗證、回傳 HTTP Status Code | 不寫複雜商業流程、不直接操作 DbContext |
| Service | 商業邏輯、權限判斷、交易流程、DTO 組裝、稽核觸發 | 不處理 HTTP 細節 |
| Repository | 查詢、新增、修改、刪除資料 | 不決定訂單、付款、庫存規則 |
| DbContext | EF Core 與 MSSQL 存取入口 | 不被 Controller 直接依賴 |
| Entity | 對應資料表欄位與關聯 | 不直接作為 API Response |
| DTO | API 輸入輸出模型 | 不代表完整資料庫結構 |
| Validator | 欄位格式、必要欄位、基本規則 | 不取代 Service 商業規則 |

---

## 3. 模組化 Service 規劃

| 模組 | 建議 Service | 主要責任 |
|---|---|---|
| 帳戶與權限 | AuthService, UserService, RoleService, PermissionService | 登入、帳號、角色、權限 |
| 商品目錄 | ProductService, ProductSkuService, ProductCategoryService | 商品維護、SKU、分類、圖片、規格 |
| 庫存 | InventoryService, WarehouseService | 庫存查詢、調整、保留、釋放 |
| 購物車與訂單 | CartService, OrderService | 購物車、建立訂單、取消訂單、狀態流轉 |
| 付款、物流與退款 | PaymentService, ShipmentService, RefundService | 付款、金流回呼、出貨、退款 |
| 優惠與促銷 | CouponService, PromotionService | 優惠券驗證、促銷、折扣計算 |
| 稽核與系統紀錄 | AuditService, LogService | 操作紀錄、資料異動、錯誤紀錄 |
| 安全與共用 | CurrentUserService, PermissionChecker, ClockService | 目前使用者、權限檢查、時間來源 |

Service 是系統規則的核心。例如「建立訂單」至少包含：

```text
1. 取得目前登入會員
2. 驗證購物車項目
3. 從資料庫重新讀取商品與 SKU 價格
4. 檢查商品是否可購買
5. 檢查庫存
6. 計算折扣
7. 建立 Orders
8. 建立 OrderItems
9. 建立 InventoryReservations
10. 建立 OrderDiscounts
11. 寫入 OrderStatusHistories
12. 使用 Transaction 確保一致性
```

---

## 4. 模組化 Repository 規劃

| 模組 | 建議 Repository | 對應資料表 |
|---|---|---|
| 帳戶與權限 | UserRepository, RoleRepository, PermissionRepository | Users, Roles, UserRoles, Permissions, RolePermissions |
| 商品目錄 | ProductRepository, ProductSkuRepository, ProductCategoryRepository | Products, ProductSkus, ProductCategories |
| 庫存 | InventoryRepository, WarehouseRepository | InventoryStocks, InventoryTransactions, Warehouses |
| 訂單 | CartRepository, OrderRepository | ShoppingCarts, Orders, OrderItems |
| 付款物流退款 | PaymentRepository, ShipmentRepository, RefundRepository | Payments, Shipments, Refunds |
| 優惠促銷 | CouponRepository, PromotionRepository | Coupons, Promotions, OrderDiscounts |
| 稽核紀錄 | AuditRepository, LogRepository | AuditLogs, AdminActionLogs, SystemErrorLogs |

Repository 可以做：

```text
查詢商品
查詢訂單明細
新增付款紀錄
更新庫存數量
寫入稽核紀錄
```

Repository 不應該做：

```text
判斷誰可以退款
決定訂單是否可出貨
決定優惠券是否可疊加
決定管理員是否可調整庫存
```

---

## 5. DTO 分類規劃

```text
MySystem.Application/
└─ DTOs/
   ├─ Storefront/
   │  ├─ Auth/
   │  ├─ Products/
   │  ├─ Cart/
   │  ├─ Orders/
   │  └─ Payments/
   ├─ Admin/
   │  ├─ Auth/
   │  ├─ Users/
   │  ├─ Products/
   │  ├─ Inventory/
   │  ├─ Orders/
   │  ├─ Payments/
   │  ├─ Shipments/
   │  ├─ Refunds/
   │  ├─ Coupons/
   │  └─ Audit/
   └─ Common/
      ├─ ApiResponse.cs
      ├─ PagedRequest.cs
      ├─ PagedResult.cs
      └─ ErrorCode.cs
```

DTO 原則：

```text
Request DTO：前端送進來的資料
Response DTO：後端回傳給前端的資料
Entity：資料庫裡真正保存的資料
```

禁止把下列欄位回傳前端：

```text
PasswordHash
SecurityStamp
Token 簽章金鑰
內部稽核欄位
完整金流機密資料
完整錯誤 Stack Trace
```

---

## 6. 權限檢查位置

正式系統不能只靠 Controller Attribute 或前端隱藏按鈕。

建議三層檢查：

```text
前端：隱藏不可用按鈕，只做 UX
Controller：[Authorize]、基本 Role / Policy
Service：檢查資料擁有者、Role、Permission、操作狀態
```

範例：會員查詢訂單

```text
Controller：要求登入
Service：確認 Order.UserId == CurrentUser.UserId
Repository：只負責查詢訂單資料
```

範例：後台退款

```text
Controller：要求後台登入
Service：確認具有 Refunds.Manage 權限
Service：確認訂單狀態可退款
Service：確認退款金額不超過可退款金額
Service：寫入 Refunds、RefundItems、Payments 狀態與稽核紀錄
```

---

## 7. Transaction 與 UnitOfWork

只要一個流程修改多張資料表，就要考慮 Transaction。

建議建立：

```text
IUnitOfWork
ITransactionService
```

典型流程：

```text
建立訂單 + 建立訂單明細 + 保留庫存
付款成功 + 寫入金流交易 + 更新訂單狀態
出貨成功 + 扣庫存 + 寫入庫存異動
退款成功 + 寫入退款明細 + 更新付款狀態
使用優惠券 + 寫入使用紀錄 + 寫入訂單折扣
```

Transaction 規則：

```text
□ Transaction 邊界放在 Service
□ Repository 不自行開多層交易
□ 同一個業務流程只應有一個主要交易範圍
□ 失敗時回滾並記錄錯誤
```

---

## 8. 冪等設計

正式系統常遇到重複送出或外部服務重送 Callback。

需要冪等的流程：

```text
□ 金流 Callback
□ 建立付款交易
□ 確認出貨
□ 建立退款
□ 使用優惠券
□ 重送訂單建立請求
```

建議做法：

```text
□ 外部交易編號設 Unique Index
□ Callback 先查是否已處理
□ 重複請求回傳相同結果或安全忽略
□ 關鍵操作可使用 Idempotency-Key
```

---

## 9. 驗證與錯誤處理

分工建議：

| 類型 | 位置 | 範例 |
|---|---|---|
| 欄位格式 | Validator | Email 格式、必填、長度 |
| 商業規則 | Service | 付款後才可出貨、退款不可超額 |
| 資料庫限制 | MSSQL / EF Core | Unique、FK、Check Constraint |
| HTTP 呈現 | Controller / Exception Handler | 400、401、403、404、409、500 |

錯誤回傳不可暴露：

```text
SQL 語法
Connection String
Stack Trace
檔案路徑
內部類別名稱
金流金鑰
```

---

## 10. 測試方向

至少需要：

```text
□ Service 單元測試
□ Repository 整合測試
□ API 整合測試
□ 權限測試
□ Transaction 回滾測試
□ 金流 Callback 重複送達測試
□ 併發下單測試
□ 庫存不可為負測試
□ 退款不可超額測試
```

---

## 11. 常見錯誤

### Controller 直接操作 DbContext

結果：商業邏輯散落、權限難以集中、交易控制困難。

### 把 Entity 直接回傳給前端

結果：敏感欄位、內部狀態、關聯資料可能外洩。

### 把訂單流程當 CRUD

結果：訂單、庫存、付款、優惠紀錄可能不一致。

### 只靠前端隱藏按鈕控權

結果：攻擊者可直接呼叫 API 越權操作。

---

## 12. 本章總結

本專案後端分層應以業務流程與安全邊界為核心。Controller 只做入口，Service 掌握規則、權限與交易，Repository 只做資料存取。正式部署前，必須確保每個重要流程都可測試、可回滾、可稽核。
