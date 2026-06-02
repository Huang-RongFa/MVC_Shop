# 01｜資料庫與 EF Core 正式對應

## 1. 本章目的

本章說明如何把既有 MSSQL 電商資料庫穩定對應到 EF Core。正式系統不能只做到「查得到資料」，還要確保欄位型別、關聯、限制、索引、交易與併發更新都符合實際營運需求。

整體流程：

```text
docs/MSSQL 資料庫設計
  ↓
MSSQL 實體資料表
  ↓
EF Core Entity
  ↓
Fluent API Configuration
  ↓
DbContext
  ↓
Repository / Service
  ↓
Controller / API DTO
```

---

## 2. 資料庫真實來源

後端應以 `docs/MSSQL` 的資料庫設計與實際建表 SQL 為基準，尤其是：

| 文件 | 用途 |
|---|---|
| `01_資料庫系統範圍與設計原則.md` | 資料庫模組與命名原則 |
| `02_帳戶角色與權限資料表設計.md` | 帳戶、角色、權限 |
| `03_商品分類SKU與規格資料表設計.md` | 商品、分類、SKU、圖片、規格 |
| `04_庫存倉庫與庫存異動資料表設計.md` | 倉庫、庫存、庫存異動 |
| `05_購物車訂單與訂單明細資料表設計.md` | 購物車、訂單、訂單明細 |
| `06_付款物流與退款資料表設計.md` | 付款、物流、退款 |
| `07_優惠券促銷與折扣資料表設計.md` | 優惠券、促銷、折扣 |
| `08_後台操作紀錄稽核與系統紀錄設計.md` | 稽核、操作、錯誤紀錄 |
| `11_mssql_create_table_script.sql` | 實際建表 SQL |
| `12_ERD文字版資料表關聯整理.md` | ERD 關聯整理 |
| `13_後端API與資料表對應規劃.md` | API 與資料表對應 |

---

## 3. EF Core 對應原則

| MSSQL | EF Core | 注意事項 |
|---|---|---|
| Table | Entity Class | 資料表通常複數，Entity 通常單數 |
| Column | Property | 型別、長度、nullable 要對齊 |
| Primary Key | HasKey | 不可只靠命名慣例猜測 |
| Foreign Key | HasOne / HasMany | 明確指定 DeleteBehavior |
| Unique Key | HasIndex().IsUnique() | 帳號、SKU、CouponCode 等應限制唯一 |
| Check Constraint | HasCheckConstraint | 狀態、金額、數量應有資料庫層保護 |
| Index | HasIndex | 查詢、排序、篩選欄位要規劃索引 |
| rowversion | IsRowVersion | 用於樂觀併發控制 |

Entity 是資料庫模型，不是 API Response。所有 API 對外輸入輸出都應使用 DTO。

---

## 4. Entity 分組

```text
MySystem.Domain/
└─ Entities/
   ├─ Accounts/
   │  ├─ User.cs
   │  ├─ Role.cs
   │  ├─ UserRole.cs
   │  ├─ Permission.cs
   │  ├─ RolePermission.cs
   │  └─ UserLoginLog.cs
   ├─ Catalog/
   │  ├─ ProductCategory.cs
   │  ├─ Product.cs
   │  ├─ ProductSku.cs
   │  ├─ ProductImage.cs
   │  ├─ ProductAttribute.cs
   │  └─ ProductAttributeValue.cs
   ├─ Inventory/
   ├─ Orders/
   ├─ Payments/
   ├─ Promotions/
   └─ Logs/
```

---

## 5. DbContext 規劃

`AppDbContext` 應集中管理 DbSet，但 Fluent API 不建議全部塞在 `OnModelCreating`。

建議結構：

```text
MySystem.Infrastructure/
└─ Data/
   ├─ AppDbContext.cs
   └─ Configurations/
      ├─ Accounts/
      │  ├─ UserConfiguration.cs
      │  └─ RoleConfiguration.cs
      ├─ Catalog/
      ├─ Inventory/
      ├─ Orders/
      ├─ Payments/
      ├─ Promotions/
      └─ Logs/
```

`AppDbContext` 至少包含：

```text
Accounts:
Users, Roles, UserRoles, Permissions, RolePermissions, UserLoginLogs

Catalog:
ProductCategories, Products, ProductSkus, ProductImages,
ProductAttributes, ProductAttributeValues

Inventory:
Warehouses, InventoryStocks, InventoryTransactions, InventoryReservations

Orders:
ShoppingCarts, ShoppingCartItems, Orders, OrderItems, OrderStatusHistories

Payments:
Payments, PaymentTransactions, Shipments, ShipmentItems, Refunds, RefundItems

Promotions:
Coupons, CouponUsages, Promotions, PromotionProducts, OrderDiscounts

Logs:
AuditLogs, AdminActionLogs, SystemErrorLogs
```

---

## 6. Migration 策略

你的專案比較接近「既有資料庫優先」或「SQL Script 作為資料庫真實來源」。

建議：

```text
1. 保留 MSSQL 建表 SQL 作為資料庫基準
2. 手動建立 Entity 與 Configuration
3. 先用測試查詢確認欄位與關聯正確
4. 不要在未確認前讓 Migration 重建正式資料庫
5. 未來若改 Code First，需要建立正式 Migration 流程與審查機制
```

正式上線時，資料庫變更不可直接手動改正式 DB，應保留：

```text
□ 變更腳本
□ 回滾腳本
□ 變更原因
□ 預估影響
□ 測試結果
□ 上線前備份紀錄
```

---

## 7. 交易一致性

電商系統最容易出問題的不是單表查詢，而是跨表流程只寫入一半。

必須使用 Transaction 的流程：

```text
□ 建立訂單 + 訂單明細 + 保留庫存 + 折扣紀錄
□ 取消訂單 + 釋放保留庫存 + 訂單狀態歷史
□ 付款成功 + 金流交易 + 更新訂單狀態
□ 建立出貨 + 出貨明細
□ 確認出貨 + 扣庫存 + 庫存異動紀錄
□ 退款成功 + 退款明細 + 更新付款狀態
□ 使用優惠券 + 使用紀錄 + 訂單折扣
```

交易原則：

```text
要成功就全部成功
任何一步失敗就全部回滾
不可只更新 Orders，卻沒有更新 OrderItems 或 InventoryReservations
```

---

## 8. 併發控制

正式部署後會有多人同時操作，需處理併發問題。

高風險情境：

```text
□ 多位顧客同時購買同一 SKU
□ 後台同時調整庫存
□ 金流 Callback 重複送達
□ 管理員同時修改同一筆商品
□ 退款流程被重複提交
```

建議做法：

| 情境 | 建議 |
|---|---|
| 商品與庫存更新 | 使用 rowversion 或條件式更新 |
| 金流 Callback | 使用交易編號唯一限制與冪等處理 |
| 退款 | 檢查可退款餘額與 RefundItems 總額 |
| 訂單狀態 | 使用狀態機規則，不允許任意跳轉 |
| 優惠券 | 使用唯一紀錄與使用次數限制 |

---

## 9. 查詢效能

正式系統需要避免查詢把網站拖垮。

基本原則：

```text
□ 列表查詢一律分頁
□ 後台報表限制日期範圍
□ 搜尋與排序欄位使用白名單
□ 使用 AsNoTracking 查詢純讀資料
□ 避免一次 Include 過深關聯
□ 大量匯出改成背景工作或限制筆數
□ 為常用查詢建立索引
```

常見需要索引的欄位：

```text
Users.Account
Products.CategoryId
Products.IsPublished
ProductSkus.SkuCode
Orders.UserId
Orders.OrderStatus
Orders.CreatedAt
Payments.OrderId
Shipments.OrderId
InventoryStocks.ProductSkuId
AuditLogs.CreatedAt
AdminActionLogs.AdminUserId
```

---

## 10. 備份與復原

正式上線前必須有資料庫備份策略。

最低要求：

```text
□ 每日完整備份
□ 重要操作前手動備份
□ 上線前備份
□ 定期測試還原
□ 備份檔加密或限制存取
□ 備份位置不要只放同一台主機
```

如果未測試還原，不能視為真正有備份。

---

## 11. EF Core 檢查清單

```text
□ Entity 欄位型別與 MSSQL 對齊
□ Required / MaxLength / Precision 設定完整
□ PK / FK / Unique / Index 對齊資料庫
□ DeleteBehavior 明確設定
□ 不直接回傳 Entity
□ 查詢列表有分頁
□ 大量查詢使用 AsNoTracking
□ 跨表流程使用 Transaction
□ 高風險流程有冪等與併發控制
□ 正式 DB 變更有腳本、備份與回滾計畫
```

---

## 12. 本章總結

EF Core 的目標不是讓 C# 能查資料而已，而是讓系統能穩定、安全、一致地操作正式資料庫。此專案應先對齊 MSSQL 資料表、關聯與限制，再建立 Repository、Service、交易控制與部署前資料庫檢查。
