# MSSQL 電商後台資料庫完整設計總文件

此文件由所有分章 Markdown 合併而成，適合直接作為專案規格書或教材。


---


# 01 電商後台資料庫系統範圍與設計原則

## 1. 系統定位

本資料庫用於支援一套全端開發的電商後台系統，主要功能包含商品管理、會員管理、訂單管理、庫存管理、付款物流管理、優惠券管理、促銷管理與後台稽核紀錄。

此設計適合用於：

- ASP.NET Core MVC
- ASP.NET Core Web API
- Vue / React 前後端分離專案
- Entity Framework Core
- Dapper
- MSSQL Server
- Azure SQL Database
- Windows Server + IIS 部署

---

## 2. 系統角色

電商後台通常會有兩種主要使用者：

| 類型 | 說明 |
|---|---|
| 前台會員 | 消費者，用於登入網站、瀏覽商品、下單、付款、查詢訂單 |
| 後台管理員 | 公司內部人員，用於管理商品、訂單、庫存、優惠、報表與權限 |

建議不要把所有帳號都混在一起管理，而是透過 `UserType` 欄位區分：

```text
UserType = Customer  前台會員
UserType = Admin     後台管理員
UserType = Staff     一般員工
```

---

## 3. 資料庫模組劃分

### 3.1 帳戶與權限模組

負責登入、身分辨識、角色管理、權限控管。

主要資料表：

```text
Users
Roles
UserRoles
Permissions
RolePermissions
UserLoginLogs
```

---

### 3.2 商品目錄模組

負責商品分類、商品主檔、SKU、圖片、規格屬性。

主要資料表：

```text
ProductCategories
Products
ProductSkus
ProductImages
ProductAttributes
ProductAttributeValues
```

---

### 3.3 庫存模組

負責倉庫、庫存數量、庫存異動與訂單保留庫存。

主要資料表：

```text
Warehouses
InventoryStocks
InventoryTransactions
InventoryReservations
```

---

### 3.4 購物車與訂單模組

負責購物車、訂單主檔、訂單明細與狀態紀錄。

主要資料表：

```text
ShoppingCarts
ShoppingCartItems
Orders
OrderItems
OrderStatusHistories
```

---

### 3.5 付款、物流與退款模組

負責付款紀錄、第三方金流交易、出貨、物流與退款。

主要資料表：

```text
Payments
PaymentTransactions
Shipments
ShipmentItems
Refunds
RefundItems
```

---

### 3.6 優惠與促銷模組

負責優惠券、促銷活動、折扣條件、使用紀錄。

主要資料表：

```text
Coupons
CouponUsages
Promotions
PromotionProducts
OrderDiscounts
```

---

### 3.7 稽核與系統紀錄模組

負責後台操作紀錄、登入紀錄、錯誤紀錄。

主要資料表：

```text
AuditLogs
AdminActionLogs
SystemErrorLogs
```

---

## 4. 命名原則

### 4.1 資料表命名

建議使用英文複數名詞：

```text
Users
Orders
OrderItems
Products
ProductSkus
```

不建議：

```text
UserData
ProductTable
OrderInfo
```

原因是後續使用 EF Core 或 API 命名時會比較一致。

---

### 4.2 主鍵命名

建議使用：

```text
UserId
ProductId
OrderId
SkuId
```

不建議每張表都只叫：

```text
Id
```

雖然 `Id` 在 EF Core 很常見，但對於大型資料庫或直接寫 SQL 時，`UserId`、`ProductId` 的可讀性較高。

---

### 4.3 外鍵命名

外鍵欄位名稱應與被參照表主鍵一致，例如：

```text
Orders.UserId -> Users.UserId
OrderItems.OrderId -> Orders.OrderId
OrderItems.SkuId -> ProductSkus.SkuId
```

---

### 4.4 業務編號命名

業務編號通常稱為 TK，可用於追蹤交易流程。

| 欄位 | 用途 |
|---|---|
| OrderNo | 訂單編號 |
| PaymentNo | 付款編號 |
| ShipmentNo | 出貨編號 |
| RefundNo | 退款編號 |
| InventoryTransactionNo | 庫存異動單號 |

例如：

```text
OrderNo = ORD202605170001
PaymentNo = PAY202605170001
ShipmentNo = SHP202605170001
```

---

## 5. 資料表共用欄位設計

多數資料表建議加入下列欄位，用於稽核與軟刪除。

| 欄位 | 型別 | 說明 |
|---|---|---|
| CreatedAt | DATETIME2 | 建立時間 |
| CreatedBy | BIGINT NULL | 建立者 |
| UpdatedAt | DATETIME2 NULL | 更新時間 |
| UpdatedBy | BIGINT NULL | 更新者 |
| IsDeleted | BIT | 是否刪除 |
| DeletedAt | DATETIME2 NULL | 刪除時間 |

---

## 6. 軟刪除設計

後台系統不建議直接刪除重要資料，例如訂單、付款、商品、庫存異動。建議使用軟刪除：

```sql
IsDeleted = 1
DeletedAt = GETDATE()
```

適合軟刪除的資料：

- 商品
- 商品分類
- 優惠券
- 促銷活動
- 後台帳號

不建議軟刪除的資料：

- 訂單明細
- 付款紀錄
- 庫存異動紀錄
- 稽核紀錄

這些資料應保留完整歷史，不應被刪除。

---

## 7. 金額欄位設計

金額欄位建議使用：

```sql
DECIMAL(18, 2)
```

不要使用 FLOAT 或 REAL，避免小數計算誤差。

例如：

```text
UnitPrice DECIMAL(18,2)
SubtotalAmount DECIMAL(18,2)
TotalAmount DECIMAL(18,2)
DiscountAmount DECIMAL(18,2)
ShippingFee DECIMAL(18,2)
```

---

## 8. 狀態欄位設計

狀態欄位可以使用 NVARCHAR 或 TINYINT。

初學或系統開發初期，建議使用 NVARCHAR，方便閱讀：

```text
OrderStatus = Pending / Paid / Shipped / Completed / Cancelled
PaymentStatus = Pending / Paid / Failed / Refunded
```

大型系統可改為狀態碼表：

```text
StatusCode
StatusName
StatusType
```

---

## 9. 資料庫設計總結

本資料庫設計核心精神如下：

1. 商品與 SKU 分開，商品是展示單位，SKU 是實際銷售與庫存單位。
2. 訂單主檔與訂單明細分開，方便統計與追蹤。
3. 庫存數量與庫存異動分開，避免只知道目前庫存，不知道變動原因。
4. 付款與訂單分開，因為一筆訂單可能有多次付款嘗試。
5. 優惠券使用紀錄獨立保存，避免優惠濫用。
6. 後台操作需留下紀錄，方便追蹤人員修改行為。


---


# 02 帳戶、角色與權限資料表設計

## 1. 模組目的

帳戶與權限模組負責管理前台會員、後台管理員、角色、權限、登入紀錄與帳戶狀態。此模組是電商後台安全性的基礎。

主要解決問題：

1. 誰可以登入系統？
2. 使用者是前台會員還是後台管理員？
3. 使用者可以操作哪些功能？
4. 誰在什麼時間登入？
5. 帳號是否被停用或鎖定？

---

## 2. Users 使用者資料表

### 2.1 用途

`Users` 用於保存所有可登入系統的帳號資料，包括前台會員與後台管理員。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| UserId | BIGINT IDENTITY | PK | N | 使用者主鍵 |
| UserNo | NVARCHAR(30) | UK/TK | N | 使用者編號，例如 USR202605170001 |
| Account | NVARCHAR(100) | UK | N | 登入帳號 |
| Email | NVARCHAR(255) | UK | Y | 電子郵件 |
| Phone | NVARCHAR(30) | UK | Y | 手機號碼 |
| PasswordHash | NVARCHAR(500) |  | N | 密碼雜湊值 |
| PasswordSalt | NVARCHAR(200) |  | Y | 密碼 Salt，若使用 Identity 可省略 |
| DisplayName | NVARCHAR(100) |  | N | 顯示名稱 |
| UserType | NVARCHAR(30) | CK | N | Customer/Admin/Staff |
| Status | NVARCHAR(30) | CK | N | Active/Inactive/Locked |
| LastLoginAt | DATETIME2 |  | Y | 最後登入時間 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 軟刪除 |

### 2.3 設計重點

- `UserId` 是系統內部使用的 PK。
- `UserNo` 是業務追蹤用 TK，可顯示於後台頁面。
- `Account`、`Email`、`Phone` 可視需求設定唯一值。
- `PasswordHash` 不可存明碼密碼。
- `UserType` 可區分前台會員與後台人員。

---

## 3. Roles 角色資料表

### 3.1 用途

`Roles` 用於定義系統角色，例如系統管理員、商品管理員、訂單管理員、客服人員、倉管人員。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| RoleId | INT IDENTITY | PK | N | 角色主鍵 |
| RoleCode | NVARCHAR(50) | UK | N | 角色代碼，例如 ADMIN |
| RoleName | NVARCHAR(100) |  | N | 角色名稱 |
| Description | NVARCHAR(500) |  | Y | 說明 |
| IsSystemRole | BIT |  | N | 是否為系統預設角色 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 3.3 建議預設角色

| RoleCode | RoleName | 說明 |
|---|---|---|
| SUPER_ADMIN | 超級管理員 | 擁有全部權限 |
| PRODUCT_MANAGER | 商品管理員 | 管理商品、分類、SKU |
| ORDER_MANAGER | 訂單管理員 | 管理訂單、付款、退款 |
| WAREHOUSE_STAFF | 倉管人員 | 管理庫存與出貨 |
| CUSTOMER_SERVICE | 客服人員 | 查詢訂單、處理退貨 |
| MARKETING_STAFF | 行銷人員 | 管理優惠券與促銷 |

---

## 4. UserRoles 使用者角色關聯表

### 4.1 用途

一個使用者可以有多個角色，一個角色也可以被多個使用者使用，因此需要多對多關聯表。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| UserRoleId | BIGINT IDENTITY | PK | N | 關聯主鍵 |
| UserId | BIGINT | FK | N | 使用者 ID |
| RoleId | INT | FK | N | 角色 ID |
| AssignedAt | DATETIME2 |  | N | 指派時間 |
| AssignedBy | BIGINT | FK | Y | 指派者 |

### 4.3 約束設計

```text
FK_UserRoles_Users: UserRoles.UserId -> Users.UserId
FK_UserRoles_Roles: UserRoles.RoleId -> Roles.RoleId
UK_UserRoles_UserId_RoleId: 避免同一使用者重複被指派同一角色
```

---

## 5. Permissions 權限資料表

### 5.1 用途

`Permissions` 用於定義系統功能權限，例如商品新增、商品修改、訂單檢視、退款核准。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| PermissionId | INT IDENTITY | PK | N | 權限主鍵 |
| PermissionCode | NVARCHAR(100) | UK | N | 權限代碼 |
| PermissionName | NVARCHAR(100) |  | N | 權限名稱 |
| ModuleName | NVARCHAR(100) |  | N | 模組名稱 |
| Description | NVARCHAR(500) |  | Y | 說明 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

### 5.3 權限代碼範例

| ModuleName | PermissionCode | PermissionName |
|---|---|---|
| Product | PRODUCT_VIEW | 檢視商品 |
| Product | PRODUCT_CREATE | 新增商品 |
| Product | PRODUCT_UPDATE | 修改商品 |
| Product | PRODUCT_DELETE | 刪除商品 |
| Order | ORDER_VIEW | 檢視訂單 |
| Order | ORDER_UPDATE_STATUS | 修改訂單狀態 |
| Order | ORDER_REFUND | 退款處理 |
| Inventory | INVENTORY_VIEW | 檢視庫存 |
| Inventory | INVENTORY_ADJUST | 調整庫存 |
| Promotion | COUPON_MANAGE | 管理優惠券 |

---

## 6. RolePermissions 角色權限關聯表

### 6.1 用途

角色與權限為多對多關係，因此透過 `RolePermissions` 管理。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| RolePermissionId | BIGINT IDENTITY | PK | N | 關聯主鍵 |
| RoleId | INT | FK | N | 角色 ID |
| PermissionId | INT | FK | N | 權限 ID |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

### 6.3 約束設計

```text
FK_RolePermissions_Roles: RolePermissions.RoleId -> Roles.RoleId
FK_RolePermissions_Permissions: RolePermissions.PermissionId -> Permissions.PermissionId
UK_RolePermissions_RoleId_PermissionId: 避免重複授權
```

---

## 7. UserLoginLogs 使用者登入紀錄表

### 7.1 用途

記錄使用者登入成功、登入失敗、登出、Token 更新等事件。

### 7.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| LoginLogId | BIGINT IDENTITY | PK | N | 登入紀錄主鍵 |
| UserId | BIGINT | FK | Y | 使用者 ID，登入失敗可能查不到使用者 |
| Account | NVARCHAR(100) |  | Y | 嘗試登入帳號 |
| LoginResult | NVARCHAR(30) | CK | N | Success/Failed/Logout |
| FailureReason | NVARCHAR(300) |  | Y | 失敗原因 |
| IpAddress | NVARCHAR(50) |  | Y | IP 位址 |
| UserAgent | NVARCHAR(500) |  | Y | 瀏覽器資訊 |
| LoginAt | DATETIME2 |  | N | 登入時間 |

---

## 8. 權限判斷流程

```text
使用者登入
    ↓
驗證帳號密碼
    ↓
取得 UserId
    ↓
查詢 UserRoles
    ↓
查詢 RolePermissions
    ↓
取得 PermissionCode 清單
    ↓
產生 JWT / Session
    ↓
後端 API 根據 PermissionCode 判斷是否可操作
```

---

## 9. 後端 API 對應建議

| API | 對應資料表 | 權限 |
|---|---|---|
| POST /api/auth/login | Users, UserLoginLogs | 無 |
| GET /api/users | Users | USER_VIEW |
| POST /api/users | Users | USER_CREATE |
| PUT /api/users/{id} | Users | USER_UPDATE |
| GET /api/roles | Roles | ROLE_VIEW |
| POST /api/roles | Roles | ROLE_CREATE |
| POST /api/roles/{id}/permissions | RolePermissions | ROLE_PERMISSION_UPDATE |

---

## 10. 本模組總結

帳戶與權限模組是整個電商後台的安全核心。建議一開始就將使用者、角色、權限拆開設計，避免後續系統功能變多時無法擴充。若只是小型系統，也可以先建立簡化版角色機制，但仍建議保留 `Roles`、`Permissions`、`RolePermissions` 的結構，後續擴充較容易。


---


# 03 商品、分類、SKU 與規格資料表設計

## 1. 模組目的

商品目錄模組是電商系統的核心，負責管理商品分類、商品主檔、商品 SKU、商品圖片、商品屬性與商品狀態。

電商系統中，商品設計最重要的觀念是：

```text
Product 商品主檔：負責展示與共同資訊
Sku 銷售規格：負責價格、庫存、條碼、實際銷售單位
```

例如：

```text
商品：經典白色 T-shirt
SKU 1：白色 / M 號
SKU 2：白色 / L 號
SKU 3：白色 / XL 號
```

因此，不建議只用一張 `Products` 表記錄所有商品資料。

---

## 2. ProductCategories 商品分類表

### 2.1 用途

`ProductCategories` 用於管理商品分類，支援多層分類，例如：

```text
服飾
├─ 男裝
│  ├─ 上衣
│  └─ 褲子
└─ 女裝
   ├─ 洋裝
   └─ 外套
```

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CategoryId | INT IDENTITY | PK | N | 分類主鍵 |
| ParentCategoryId | INT | FK | Y | 上層分類 ID |
| CategoryCode | NVARCHAR(50) | UK | N | 分類代碼 |
| CategoryName | NVARCHAR(100) |  | N | 分類名稱 |
| Description | NVARCHAR(500) |  | Y | 分類說明 |
| SortOrder | INT |  | N | 排序 |
| IsActive | BIT |  | N | 是否啟用 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 2.3 關聯設計

```text
ProductCategories.ParentCategoryId -> ProductCategories.CategoryId
Products.CategoryId -> ProductCategories.CategoryId
```

---

## 3. Products 商品主檔

### 3.1 用途

`Products` 用於保存商品共同資訊，例如商品名稱、說明、品牌、分類、上下架狀態。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ProductId | BIGINT IDENTITY | PK | N | 商品主鍵 |
| ProductNo | NVARCHAR(30) | UK/TK | N | 商品編號，例如 PRD202605170001 |
| CategoryId | INT | FK | N | 商品分類 |
| ProductName | NVARCHAR(200) | IDX | N | 商品名稱 |
| BrandName | NVARCHAR(100) |  | Y | 品牌名稱 |
| ShortDescription | NVARCHAR(500) |  | Y | 短描述 |
| FullDescription | NVARCHAR(MAX) |  | Y | 完整描述 |
| Status | NVARCHAR(30) | CK | N | Draft/Active/Inactive/Archived |
| IsFeatured | BIT |  | N | 是否推薦商品 |
| PublishedAt | DATETIME2 |  | Y | 上架時間 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 3.3 商品狀態建議

| 狀態 | 說明 |
|---|---|
| Draft | 草稿，尚未上架 |
| Active | 已上架，可販售 |
| Inactive | 下架，不可販售 |
| Archived | 封存，不再使用 |

---

## 4. ProductSkus 商品 SKU 表

### 4.1 用途

`ProductSkus` 是實際銷售單位，通常會包含價格、規格、條碼與銷售狀態。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| SkuId | BIGINT IDENTITY | PK | N | SKU 主鍵 |
| ProductId | BIGINT | FK | N | 商品 ID |
| SkuNo | NVARCHAR(50) | UK/TK | N | SKU 編號 |
| Barcode | NVARCHAR(100) | UK | Y | 條碼 |
| SkuName | NVARCHAR(200) |  | N | SKU 名稱 |
| SpecText | NVARCHAR(500) |  | Y | 規格文字，例如 白色/M |
| ListPrice | DECIMAL(18,2) | CK | N | 原價 |
| SalePrice | DECIMAL(18,2) | CK | N | 售價 |
| CostPrice | DECIMAL(18,2) |  | Y | 成本價 |
| Weight | DECIMAL(18,3) |  | Y | 重量 |
| Status | NVARCHAR(30) | CK | N | Active/Inactive |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 4.3 價格檢查限制

```text
ListPrice >= 0
SalePrice >= 0
CostPrice >= 0
```

---

## 5. ProductImages 商品圖片表

### 5.1 用途

商品可能有多張圖片，例如主圖、細節圖、情境圖。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ImageId | BIGINT IDENTITY | PK | N | 圖片主鍵 |
| ProductId | BIGINT | FK | N | 商品 ID |
| SkuId | BIGINT | FK | Y | SKU ID，若圖片屬於特定規格才填 |
| ImageUrl | NVARCHAR(1000) |  | N | 圖片網址或路徑 |
| AltText | NVARCHAR(200) |  | Y | 替代文字 |
| IsMainImage | BIT |  | N | 是否主圖 |
| SortOrder | INT |  | N | 排序 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 6. ProductAttributes 商品屬性表

### 6.1 用途

`ProductAttributes` 用於定義商品規格屬性，例如顏色、尺寸、材質。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AttributeId | INT IDENTITY | PK | N | 屬性主鍵 |
| AttributeCode | NVARCHAR(50) | UK | N | 屬性代碼 |
| AttributeName | NVARCHAR(100) |  | N | 屬性名稱 |
| InputType | NVARCHAR(30) | CK | N | Text/Select/Number |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

### 6.3 範例

| AttributeCode | AttributeName | InputType |
|---|---|---|
| COLOR | 顏色 | Select |
| SIZE | 尺寸 | Select |
| MATERIAL | 材質 | Text |

---

## 7. ProductAttributeValues 商品屬性值表

### 7.1 用途

此表保存某個 SKU 的屬性值。

### 7.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AttributeValueId | BIGINT IDENTITY | PK | N | 屬性值主鍵 |
| SkuId | BIGINT | FK | N | SKU ID |
| AttributeId | INT | FK | N | 屬性 ID |
| AttributeValue | NVARCHAR(200) |  | N | 屬性值 |

### 7.3 範例

| SkuNo | AttributeName | AttributeValue |
|---|---|---|
| SKU001 | 顏色 | 白色 |
| SKU001 | 尺寸 | M |
| SKU002 | 顏色 | 白色 |
| SKU002 | 尺寸 | L |

---

## 8. 商品模組關聯

```text
ProductCategories 1 ── N Products
Products 1 ── N ProductSkus
Products 1 ── N ProductImages
ProductSkus 1 ── N ProductImages
ProductSkus 1 ── N ProductAttributeValues
ProductAttributes 1 ── N ProductAttributeValues
```

---

## 9. 商品後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 商品分類管理 | ProductCategories |
| 商品新增/修改 | Products, ProductSkus, ProductImages |
| SKU 管理 | ProductSkus, ProductAttributeValues |
| 商品上下架 | Products.Status, ProductSkus.Status |
| 商品圖片排序 | ProductImages.SortOrder |
| 商品搜尋 | Products.ProductName, ProductSkus.SkuNo, Barcode |

---

## 10. 商品模組總結

商品模組的設計重點在於「商品主檔」與「SKU」的分離。商品主檔負責描述商品，SKU 負責實際販售。這樣可以支援多規格、多價格、多庫存與多圖片的電商需求。


---


# 04 庫存、倉庫與庫存異動資料表設計

## 1. 模組目的

庫存模組負責記錄商品 SKU 在不同倉庫中的可售數量、保留數量、安全庫存與庫存異動紀錄。

電商庫存不能只用一個欄位記錄數量，因為實際系統會遇到：

1. 多倉庫庫存。
2. 訂單成立後需保留庫存。
3. 付款失敗或取消訂單需釋放庫存。
4. 出貨後才正式扣除庫存。
5. 退貨後可能補回庫存。
6. 後台人員可能手動調整庫存。

因此庫存設計建議分成：

```text
InventoryStocks：目前庫存狀態
InventoryTransactions：庫存異動歷史
InventoryReservations：訂單保留庫存
```

---

## 2. Warehouses 倉庫資料表

### 2.1 用途

`Warehouses` 用於記錄倉庫或門市位置。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| WarehouseId | INT IDENTITY | PK | N | 倉庫主鍵 |
| WarehouseCode | NVARCHAR(50) | UK | N | 倉庫代碼 |
| WarehouseName | NVARCHAR(100) |  | N | 倉庫名稱 |
| Address | NVARCHAR(500) |  | Y | 倉庫地址 |
| ContactName | NVARCHAR(100) |  | Y | 聯絡人 |
| ContactPhone | NVARCHAR(30) |  | Y | 聯絡電話 |
| IsActive | BIT |  | N | 是否啟用 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 3. InventoryStocks 庫存現況表

### 3.1 用途

`InventoryStocks` 記錄每個 SKU 在每個倉庫的庫存狀態。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| InventoryStockId | BIGINT IDENTITY | PK | N | 庫存主鍵 |
| WarehouseId | INT | FK | N | 倉庫 ID |
| SkuId | BIGINT | FK | N | SKU ID |
| OnHandQty | INT | CK | N | 實際庫存數量 |
| ReservedQty | INT | CK | N | 已保留數量 |
| AvailableQty | AS | Computed | N | 可售數量，OnHandQty - ReservedQty |
| SafetyStockQty | INT | CK | N | 安全庫存 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |

### 3.3 重要計算

```text
AvailableQty = OnHandQty - ReservedQty
```

建議使用 Computed Column：

```sql
AvailableQty AS (OnHandQty - ReservedQty)
```

### 3.4 約束設計

```text
UK_InventoryStocks_WarehouseId_SkuId
```

避免同一個 SKU 在同一個倉庫重複建立庫存資料。

---

## 4. InventoryTransactions 庫存異動表

### 4.1 用途

`InventoryTransactions` 用於記錄庫存變動歷史，例如進貨、出貨、退貨、調整、保留、釋放。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| InventoryTransactionId | BIGINT IDENTITY | PK | N | 異動主鍵 |
| TransactionNo | NVARCHAR(50) | UK/TK | N | 異動單號 |
| WarehouseId | INT | FK | N | 倉庫 ID |
| SkuId | BIGINT | FK | N | SKU ID |
| TransactionType | NVARCHAR(30) | CK | N | In/Out/Reserve/Release/Adjust/Return |
| Quantity | INT | CK | N | 異動數量，可正可負，或依 Type 判斷 |
| BeforeQty | INT |  | N | 異動前庫存 |
| AfterQty | INT |  | N | 異動後庫存 |
| ReferenceType | NVARCHAR(50) |  | Y | 來源類型，例如 Order/Refund/Manual |
| ReferenceId | BIGINT |  | Y | 來源 ID |
| Remark | NVARCHAR(500) |  | Y | 備註 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| CreatedBy | BIGINT | FK | Y | 操作者 |

### 4.3 異動類型範例

| TransactionType | 說明 |
|---|---|
| In | 進貨入庫 |
| Out | 出貨扣庫 |
| Reserve | 訂單保留庫存 |
| Release | 取消訂單釋放庫存 |
| Adjust | 人工調整 |
| Return | 退貨入庫 |

---

## 5. InventoryReservations 保留庫存表

### 5.1 用途

當顧客下單但尚未完成付款或出貨時，系統可以先保留庫存，避免超賣。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ReservationId | BIGINT IDENTITY | PK | N | 保留庫存主鍵 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| OrderItemId | BIGINT | FK | N | 訂單明細 ID |
| WarehouseId | INT | FK | N | 倉庫 ID |
| SkuId | BIGINT | FK | N | SKU ID |
| ReservedQty | INT | CK | N | 保留數量 |
| Status | NVARCHAR(30) | CK | N | Reserved/Released/Consumed |
| ReservedAt | DATETIME2 |  | N | 保留時間 |
| ReleasedAt | DATETIME2 |  | Y | 釋放時間 |
| ConsumedAt | DATETIME2 |  | Y | 實際扣除時間 |

### 5.3 狀態說明

| Status | 說明 |
|---|---|
| Reserved | 已保留，尚未出貨 |
| Released | 已釋放，例如訂單取消 |
| Consumed | 已消耗，例如出貨扣庫 |

---

## 6. 庫存流程設計

### 6.1 下單時保留庫存

```text
顧客送出訂單
    ↓
檢查 AvailableQty 是否足夠
    ↓
建立 Orders / OrderItems
    ↓
增加 InventoryStocks.ReservedQty
    ↓
建立 InventoryReservations
    ↓
建立 InventoryTransactions，Type = Reserve
```

---

### 6.2 取消訂單釋放庫存

```text
訂單取消
    ↓
查詢 InventoryReservations
    ↓
減少 InventoryStocks.ReservedQty
    ↓
更新 InventoryReservations.Status = Released
    ↓
建立 InventoryTransactions，Type = Release
```

---

### 6.3 出貨扣除庫存

```text
訂單準備出貨
    ↓
查詢保留庫存
    ↓
OnHandQty 減少
    ↓
ReservedQty 減少
    ↓
InventoryReservations.Status = Consumed
    ↓
建立 InventoryTransactions，Type = Out
```

---

## 7. 後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 倉庫管理 | Warehouses |
| 庫存查詢 | InventoryStocks, ProductSkus, Products |
| 庫存調整 | InventoryStocks, InventoryTransactions |
| 訂單保留庫存 | InventoryReservations, InventoryStocks |
| 庫存異動查詢 | InventoryTransactions |
| 安全庫存提醒 | InventoryStocks.SafetyStockQty |

---

## 8. 庫存模組總結

庫存模組應重視「目前庫存」與「異動歷史」的分離。`InventoryStocks` 負責查詢目前狀態，`InventoryTransactions` 負責追蹤所有變動原因，`InventoryReservations` 負責避免下單與付款流程中的超賣問題。


---


# 05 購物車、訂單與訂單明細資料表設計

## 1. 模組目的

購物車與訂單模組負責從顧客加入商品、修改購物車、送出訂單、訂單狀態變更到訂單完成的完整流程。

訂單資料是電商系統中最重要的交易資料，因此設計上必須注意：

1. 訂單成立當下的商品價格要固定保存。
2. 即使商品後續改名或改價，訂單歷史不能被影響。
3. 訂單主檔與訂單明細要分開。
4. 訂單狀態變更要保留歷史紀錄。
5. 優惠、付款、物流、退款應拆成獨立模組。

---

## 2. ShoppingCarts 購物車主檔

### 2.1 用途

`ShoppingCarts` 用於保存使用者目前的購物車。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CartId | BIGINT IDENTITY | PK | N | 購物車主鍵 |
| UserId | BIGINT | FK | N | 使用者 ID |
| CartStatus | NVARCHAR(30) | CK | N | Active/Converted/Abandoned |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |

### 2.3 狀態說明

| CartStatus | 說明 |
|---|---|
| Active | 目前使用中的購物車 |
| Converted | 已轉成訂單 |
| Abandoned | 長時間未使用 |

---

## 3. ShoppingCartItems 購物車明細

### 3.1 用途

保存購物車內的商品 SKU 與數量。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CartItemId | BIGINT IDENTITY | PK | N | 購物車明細主鍵 |
| CartId | BIGINT | FK | N | 購物車 ID |
| SkuId | BIGINT | FK | N | SKU ID |
| Quantity | INT | CK | N | 數量 |
| UnitPrice | DECIMAL(18,2) | CK | N | 加入購物車當下單價 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| UpdatedAt | DATETIME2 |  | Y | 更新時間 |

### 3.3 約束設計

```text
UK_ShoppingCartItems_CartId_SkuId
```

避免同一購物車重複建立相同 SKU 明細，可改為更新數量。

---

## 4. Orders 訂單主檔

### 4.1 用途

`Orders` 保存訂單的整體資訊，例如訂單編號、會員、狀態、金額、收件資訊。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| OrderId | BIGINT IDENTITY | PK | N | 訂單主鍵 |
| OrderNo | NVARCHAR(50) | UK/TK | N | 訂單編號 |
| UserId | BIGINT | FK | N | 下單會員 |
| OrderStatus | NVARCHAR(30) | CK | N | Pending/Paid/Processing/Shipped/Completed/Cancelled |
| PaymentStatus | NVARCHAR(30) | CK | N | Pending/Paid/Failed/Refunded/PartialRefunded |
| ShippingStatus | NVARCHAR(30) | CK | N | Pending/Preparing/Shipped/Delivered/Returned |
| SubtotalAmount | DECIMAL(18,2) | CK | N | 商品小計 |
| DiscountAmount | DECIMAL(18,2) | CK | N | 折扣金額 |
| ShippingFee | DECIMAL(18,2) | CK | N | 運費 |
| TotalAmount | DECIMAL(18,2) | CK | N | 訂單總額 |
| ReceiverName | NVARCHAR(100) |  | N | 收件人 |
| ReceiverPhone | NVARCHAR(30) |  | N | 收件電話 |
| ReceiverAddress | NVARCHAR(500) |  | N | 收件地址 |
| Remark | NVARCHAR(500) |  | Y | 備註 |
| OrderedAt | DATETIME2 |  | N | 下單時間 |
| PaidAt | DATETIME2 |  | Y | 付款時間 |
| CompletedAt | DATETIME2 |  | Y | 完成時間 |
| CancelledAt | DATETIME2 |  | Y | 取消時間 |

---

## 5. OrderItems 訂單明細

### 5.1 用途

保存訂單中的每一個商品 SKU。訂單明細必須保存下單當下的商品資訊，避免商品主檔後續變動影響訂單歷史。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| OrderItemId | BIGINT IDENTITY | PK | N | 訂單明細主鍵 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| ProductId | BIGINT | FK | N | 商品 ID |
| SkuId | BIGINT | FK | N | SKU ID |
| ProductNameSnapshot | NVARCHAR(200) |  | N | 下單當下商品名稱 |
| SkuNameSnapshot | NVARCHAR(200) |  | N | 下單當下 SKU 名稱 |
| SkuNoSnapshot | NVARCHAR(50) |  | N | 下單當下 SKU 編號 |
| UnitPrice | DECIMAL(18,2) | CK | N | 單價 |
| Quantity | INT | CK | N | 數量 |
| DiscountAmount | DECIMAL(18,2) | CK | N | 單品折扣 |
| SubtotalAmount | DECIMAL(18,2) | CK | N | 小計 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

### 5.3 為什麼要 Snapshot

假設顧客今天購買：

```text
商品名稱：經典白色 T-shirt
價格：399
```

若一個月後商品改名或價格改成 499，歷史訂單仍應顯示當時購買的資料。因此訂單明細需要保存 Snapshot 欄位。

---

## 6. OrderStatusHistories 訂單狀態歷史表

### 6.1 用途

記錄訂單每次狀態變更的歷史。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| OrderStatusHistoryId | BIGINT IDENTITY | PK | N | 歷史主鍵 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| OldStatus | NVARCHAR(30) |  | Y | 原狀態 |
| NewStatus | NVARCHAR(30) |  | N | 新狀態 |
| ChangedReason | NVARCHAR(500) |  | Y | 變更原因 |
| ChangedAt | DATETIME2 |  | N | 變更時間 |
| ChangedBy | BIGINT | FK | Y | 操作者 |

---

## 7. 訂單狀態流程

```text
Pending 待付款
    ↓
Paid 已付款
    ↓
Processing 處理中
    ↓
Shipped 已出貨
    ↓
Completed 已完成
```

取消流程：

```text
Pending / Paid / Processing
    ↓
Cancelled 已取消
```

退貨退款流程：

```text
Completed / Shipped
    ↓
RefundRequested 申請退款
    ↓
Refunded 已退款
```

---

## 8. 建立訂單流程

```text
使用者送出購物車
    ↓
後端驗證登入狀態
    ↓
查詢 ShoppingCartItems
    ↓
檢查 SKU 是否上架
    ↓
檢查庫存 AvailableQty
    ↓
計算商品小計
    ↓
套用優惠券與促銷
    ↓
計算運費與總金額
    ↓
建立 Orders
    ↓
建立 OrderItems
    ↓
保留庫存 InventoryReservations
    ↓
購物車狀態改為 Converted
    ↓
回傳 OrderNo 給前端
```

---

## 9. 後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 訂單列表 | Orders |
| 訂單明細 | Orders, OrderItems |
| 修改訂單狀態 | Orders, OrderStatusHistories |
| 付款狀態查詢 | Orders, Payments |
| 出貨處理 | Orders, Shipments |
| 取消訂單 | Orders, InventoryReservations |
| 訂單報表 | Orders, OrderItems |

---

## 10. 訂單模組總結

訂單模組的重點是資料不可被商品主檔變動影響。因此 `OrderItems` 必須保存商品名稱、SKU 名稱、單價等 Snapshot 欄位。訂單狀態變更也應保存於 `OrderStatusHistories`，方便後台追蹤處理過程。


---


# 06 付款、物流與退款資料表設計

## 1. 模組目的

付款、物流與退款模組負責訂單成立後的交易處理流程。此模組不建議直接寫在 `Orders` 一張表中，因為一筆訂單可能會發生：

1. 多次付款嘗試。
2. 付款失敗後重新付款。
3. 部分退款。
4. 分批出貨。
5. 物流狀態多次更新。

因此建議拆成獨立資料表。

---

## 2. Payments 付款主檔

### 2.1 用途

`Payments` 保存訂單付款資訊，例如付款方式、付款狀態、付款金額。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| PaymentId | BIGINT IDENTITY | PK | N | 付款主鍵 |
| PaymentNo | NVARCHAR(50) | UK/TK | N | 付款編號 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| PaymentMethod | NVARCHAR(50) | CK | N | CreditCard/ATM/COD/LinePay/ApplePay |
| PaymentStatus | NVARCHAR(30) | CK | N | Pending/Paid/Failed/Cancelled/Refunded |
| Amount | DECIMAL(18,2) | CK | N | 付款金額 |
| PaidAt | DATETIME2 |  | Y | 付款成功時間 |
| ExpiredAt | DATETIME2 |  | Y | 付款期限 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 3. PaymentTransactions 金流交易紀錄

### 3.1 用途

保存與第三方金流平台的交易紀錄，例如交易代碼、回傳訊息、授權碼。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| PaymentTransactionId | BIGINT IDENTITY | PK | N | 交易紀錄主鍵 |
| PaymentId | BIGINT | FK | N | 付款 ID |
| ProviderName | NVARCHAR(100) |  | N | 金流商，例如 ECPay/NewebPay/Stripe |
| ProviderTransactionNo | NVARCHAR(100) | IDX | Y | 金流商交易編號 |
| TransactionStatus | NVARCHAR(30) | CK | N | Success/Failed/Pending |
| RequestPayload | NVARCHAR(MAX) |  | Y | 請求內容 |
| ResponsePayload | NVARCHAR(MAX) |  | Y | 回傳內容 |
| ErrorCode | NVARCHAR(50) |  | Y | 錯誤代碼 |
| ErrorMessage | NVARCHAR(500) |  | Y | 錯誤訊息 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 4. Shipments 出貨主檔

### 4.1 用途

`Shipments` 保存訂單出貨資訊。一筆訂單可能分批出貨，因此出貨資料應獨立於訂單。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ShipmentId | BIGINT IDENTITY | PK | N | 出貨主鍵 |
| ShipmentNo | NVARCHAR(50) | UK/TK | N | 出貨編號 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| WarehouseId | INT | FK | Y | 出貨倉庫 |
| CarrierName | NVARCHAR(100) |  | Y | 物流商 |
| TrackingNo | NVARCHAR(100) | IDX | Y | 物流追蹤碼 |
| ShipmentStatus | NVARCHAR(30) | CK | N | Preparing/Shipped/Delivered/Returned/Cancelled |
| ShippedAt | DATETIME2 |  | Y | 出貨時間 |
| DeliveredAt | DATETIME2 |  | Y | 送達時間 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 5. ShipmentItems 出貨明細

### 5.1 用途

記錄每次出貨包含哪些訂單明細與數量。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ShipmentItemId | BIGINT IDENTITY | PK | N | 出貨明細主鍵 |
| ShipmentId | BIGINT | FK | N | 出貨 ID |
| OrderItemId | BIGINT | FK | N | 訂單明細 ID |
| Quantity | INT | CK | N | 出貨數量 |

---

## 6. Refunds 退款主檔

### 6.1 用途

保存退款申請與退款狀態。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| RefundId | BIGINT IDENTITY | PK | N | 退款主鍵 |
| RefundNo | NVARCHAR(50) | UK/TK | N | 退款編號 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| PaymentId | BIGINT | FK | Y | 對應付款 |
| RefundStatus | NVARCHAR(30) | CK | N | Requested/Approved/Rejected/Refunded |
| RefundAmount | DECIMAL(18,2) | CK | N | 退款金額 |
| Reason | NVARCHAR(500) |  | Y | 退款原因 |
| RequestedAt | DATETIME2 |  | N | 申請時間 |
| ApprovedAt | DATETIME2 |  | Y | 核准時間 |
| RefundedAt | DATETIME2 |  | Y | 完成退款時間 |
| CreatedBy | BIGINT | FK | Y | 建立者 |

---

## 7. RefundItems 退款明細

### 7.1 用途

記錄退款涉及哪些訂單商品。

### 7.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| RefundItemId | BIGINT IDENTITY | PK | N | 退款明細主鍵 |
| RefundId | BIGINT | FK | N | 退款 ID |
| OrderItemId | BIGINT | FK | N | 訂單明細 ID |
| Quantity | INT | CK | N | 退款數量 |
| RefundAmount | DECIMAL(18,2) | CK | N | 明細退款金額 |

---

## 8. 付款流程

```text
訂單成立
    ↓
建立 Payments，Status = Pending
    ↓
呼叫第三方金流
    ↓
建立 PaymentTransactions
    ↓
付款成功
    ↓
Payments.Status = Paid
Orders.PaymentStatus = Paid
Orders.OrderStatus = Paid
    ↓
進入出貨流程
```

---

## 9. 出貨流程

```text
訂單已付款
    ↓
後台建立 Shipments
    ↓
選擇出貨倉庫
    ↓
建立 ShipmentItems
    ↓
扣除庫存 OnHandQty 與 ReservedQty
    ↓
ShipmentStatus = Shipped
Orders.ShippingStatus = Shipped
```

---

## 10. 退款流程

```text
顧客申請退款
    ↓
建立 Refunds，Status = Requested
    ↓
客服或管理員審核
    ↓
Approved / Rejected
    ↓
若核准，呼叫金流退款 API
    ↓
Refunds.Status = Refunded
Payments.Status = Refunded 或 PartialRefunded
Orders.PaymentStatus = Refunded 或 PartialRefunded
```

---

## 11. 本模組總結

付款、物流與退款應與訂單主檔分離，因為交易流程具有多次嘗試、多狀態、多紀錄的特性。這樣的設計能支援金流串接、物流追蹤、部分退款、分批出貨與後台稽核。


---


# 07 優惠券、促銷與折扣資料表設計

## 1. 模組目的

優惠模組負責管理電商系統中的優惠券、促銷活動、折扣條件與實際使用紀錄。此模組應能支援：

1. 滿額折扣。
2. 固定金額折扣。
3. 百分比折扣。
4. 指定商品優惠。
5. 指定分類優惠。
6. 每人限用次數。
7. 總使用次數限制。
8. 優惠開始與結束時間。
9. 訂單實際折扣紀錄。

---

## 2. Coupons 優惠券資料表

### 2.1 用途

`Coupons` 用於管理優惠券代碼與使用條件。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CouponId | BIGINT IDENTITY | PK | N | 優惠券主鍵 |
| CouponCode | NVARCHAR(50) | UK/TK | N | 優惠碼 |
| CouponName | NVARCHAR(100) |  | N | 優惠券名稱 |
| DiscountType | NVARCHAR(30) | CK | N | Amount/Percent |
| DiscountValue | DECIMAL(18,2) | CK | N | 折扣值 |
| MinOrderAmount | DECIMAL(18,2) | CK | N | 最低訂單金額 |
| MaxDiscountAmount | DECIMAL(18,2) |  | Y | 百分比折扣上限 |
| TotalUsageLimit | INT |  | Y | 總使用次數限制 |
| PerUserUsageLimit | INT |  | Y | 每人使用次數限制 |
| UsedCount | INT |  | N | 已使用次數 |
| StartAt | DATETIME2 |  | N | 開始時間 |
| EndAt | DATETIME2 |  | N | 結束時間 |
| Status | NVARCHAR(30) | CK | N | Active/Inactive/Expired |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

### 2.3 折扣類型說明

| DiscountType | 說明 | 範例 |
|---|---|---|
| Amount | 固定金額折扣 | 折 100 元 |
| Percent | 百分比折扣 | 打 9 折 |

---

## 3. CouponUsages 優惠券使用紀錄

### 3.1 用途

記錄優惠券被誰、在哪一筆訂單使用。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| CouponUsageId | BIGINT IDENTITY | PK | N | 使用紀錄主鍵 |
| CouponId | BIGINT | FK | N | 優惠券 ID |
| UserId | BIGINT | FK | N | 使用者 ID |
| OrderId | BIGINT | FK | N | 訂單 ID |
| DiscountAmount | DECIMAL(18,2) | CK | N | 實際折扣金額 |
| UsedAt | DATETIME2 |  | N | 使用時間 |

### 3.3 使用限制檢查

使用優惠券前需檢查：

```text
優惠券是否存在
優惠券是否啟用
目前時間是否在 StartAt 與 EndAt 之間
訂單金額是否達到 MinOrderAmount
UsedCount 是否小於 TotalUsageLimit
使用者已用次數是否小於 PerUserUsageLimit
```

---

## 4. Promotions 促銷活動資料表

### 4.1 用途

`Promotions` 用於管理不需要輸入優惠碼的促銷活動，例如全站滿千折百、指定商品特價。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| PromotionId | BIGINT IDENTITY | PK | N | 促銷主鍵 |
| PromotionCode | NVARCHAR(50) | UK/TK | N | 促銷代碼 |
| PromotionName | NVARCHAR(100) |  | N | 促銷名稱 |
| PromotionType | NVARCHAR(30) | CK | N | OrderDiscount/ProductDiscount/FreeShipping |
| DiscountType | NVARCHAR(30) | CK | N | Amount/Percent |
| DiscountValue | DECIMAL(18,2) | CK | N | 折扣值 |
| MinOrderAmount | DECIMAL(18,2) | CK | N | 最低訂單金額 |
| StartAt | DATETIME2 |  | N | 開始時間 |
| EndAt | DATETIME2 |  | N | 結束時間 |
| Status | NVARCHAR(30) | CK | N | Active/Inactive/Expired |
| CreatedAt | DATETIME2 |  | N | 建立時間 |
| IsDeleted | BIT |  | N | 是否刪除 |

---

## 5. PromotionProducts 促銷商品關聯表

### 5.1 用途

記錄促銷活動適用於哪些商品或 SKU。

### 5.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| PromotionProductId | BIGINT IDENTITY | PK | N | 關聯主鍵 |
| PromotionId | BIGINT | FK | N | 促銷 ID |
| ProductId | BIGINT | FK | Y | 商品 ID |
| SkuId | BIGINT | FK | Y | SKU ID |

### 5.3 設計說明

若促銷套用整個商品，填 `ProductId`。若只套用特定 SKU，填 `SkuId`。

---

## 6. OrderDiscounts 訂單折扣紀錄

### 6.1 用途

保存訂單實際套用的折扣紀錄，包含優惠券與促銷活動。

### 6.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| OrderDiscountId | BIGINT IDENTITY | PK | N | 折扣紀錄主鍵 |
| OrderId | BIGINT | FK | N | 訂單 ID |
| DiscountSourceType | NVARCHAR(30) | CK | N | Coupon/Promotion/Manual |
| CouponId | BIGINT | FK | Y | 優惠券 ID |
| PromotionId | BIGINT | FK | Y | 促銷 ID |
| DiscountName | NVARCHAR(100) |  | N | 折扣名稱 Snapshot |
| DiscountAmount | DECIMAL(18,2) | CK | N | 折扣金額 |
| CreatedAt | DATETIME2 |  | N | 建立時間 |

---

## 7. 優惠券計算流程

```text
使用者輸入 CouponCode
    ↓
查詢 Coupons
    ↓
檢查狀態、時間、金額、使用次數
    ↓
計算折扣金額
    ↓
更新訂單 DiscountAmount
    ↓
建立 OrderDiscounts
    ↓
建立 CouponUsages
    ↓
更新 Coupons.UsedCount
```

---

## 8. 折扣計算範例

### 8.1 固定金額折扣

```text
訂單金額：1200
優惠券：折 100
折扣後金額：1100
```

### 8.2 百分比折扣

```text
訂單金額：1200
優惠券：9 折
折扣金額：120
折扣後金額：1080
```

若有折扣上限：

```text
訂單金額：5000
優惠券：9 折，最多折 300
原本折扣：500
實際折扣：300
```

---

## 9. 後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 優惠券管理 | Coupons |
| 優惠券使用查詢 | CouponUsages |
| 促銷活動管理 | Promotions |
| 指定商品促銷 | PromotionProducts |
| 訂單折扣查詢 | OrderDiscounts |
| 折扣報表 | Coupons, CouponUsages, OrderDiscounts |

---

## 10. 優惠模組總結

優惠模組設計的關鍵是「條件」與「紀錄」分開。`Coupons`、`Promotions` 負責定義優惠規則，`CouponUsages`、`OrderDiscounts` 負責保存實際使用結果。這樣才能避免後續優惠規則修改後影響歷史訂單。


---


# 08 後台操作紀錄、稽核與系統紀錄設計

## 1. 模組目的

稽核模組負責記錄後台使用者在系統中的重要操作，例如新增商品、修改價格、調整庫存、取消訂單、核准退款等。

電商後台牽涉金額、庫存、訂單與個資，因此必須能回答下列問題：

1. 誰修改了商品價格？
2. 誰調整了庫存？
3. 誰取消了訂單？
4. 誰核准退款？
5. 使用者從哪個 IP 登入？
6. 系統發生錯誤時，錯誤內容是什麼？

---

## 2. AuditLogs 通用資料異動紀錄

### 2.1 用途

`AuditLogs` 用於記錄資料表層級的新增、修改、刪除行為。

### 2.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AuditLogId | BIGINT IDENTITY | PK | N | 稽核紀錄主鍵 |
| TableName | NVARCHAR(100) | IDX | N | 資料表名稱 |
| RecordId | NVARCHAR(100) | IDX | N | 被操作資料 ID |
| ActionType | NVARCHAR(30) | CK | N | Insert/Update/Delete |
| OldValueJson | NVARCHAR(MAX) |  | Y | 修改前資料 JSON |
| NewValueJson | NVARCHAR(MAX) |  | Y | 修改後資料 JSON |
| ChangedBy | BIGINT | FK | Y | 操作者 |
| ChangedAt | DATETIME2 | IDX | N | 操作時間 |
| IpAddress | NVARCHAR(50) |  | Y | IP 位址 |

### 2.3 設計說明

`OldValueJson` 與 `NewValueJson` 可以保存資料異動前後內容，例如：

```json
{
  "SalePrice": 399,
  "Status": "Active"
}
```

---

## 3. AdminActionLogs 後台操作紀錄

### 3.1 用途

`AdminActionLogs` 用於記錄較高層級的後台操作，例如「商品上架」、「訂單取消」、「退款核准」。

### 3.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| AdminActionLogId | BIGINT IDENTITY | PK | N | 操作紀錄主鍵 |
| UserId | BIGINT | FK | N | 操作者 |
| ModuleName | NVARCHAR(100) | IDX | N | 模組名稱 |
| ActionName | NVARCHAR(100) | IDX | N | 操作名稱 |
| TargetType | NVARCHAR(100) |  | Y | 目標類型，例如 Product/Order |
| TargetId | NVARCHAR(100) |  | Y | 目標 ID |
| Description | NVARCHAR(1000) |  | Y | 操作描述 |
| IpAddress | NVARCHAR(50) |  | Y | IP 位址 |
| UserAgent | NVARCHAR(500) |  | Y | 瀏覽器資訊 |
| CreatedAt | DATETIME2 | IDX | N | 建立時間 |

### 3.3 操作範例

| ModuleName | ActionName | Description |
|---|---|---|
| Product | UpdatePrice | 修改商品售價 |
| Product | PublishProduct | 商品上架 |
| Inventory | AdjustStock | 手動調整庫存 |
| Order | CancelOrder | 取消訂單 |
| Refund | ApproveRefund | 核准退款 |
| User | LockUser | 鎖定帳號 |

---

## 4. SystemErrorLogs 系統錯誤紀錄

### 4.1 用途

記錄系統錯誤，方便除錯與監控。

### 4.2 欄位設計

| 欄位 | 型別 | Key | Null | 說明 |
|---|---|---|---|---|
| ErrorLogId | BIGINT IDENTITY | PK | N | 錯誤紀錄主鍵 |
| ErrorLevel | NVARCHAR(30) | CK | N | Info/Warning/Error/Critical |
| Source | NVARCHAR(200) |  | Y | 錯誤來源，例如 API 名稱 |
| Message | NVARCHAR(1000) |  | N | 錯誤訊息 |
| StackTrace | NVARCHAR(MAX) |  | Y | 堆疊資訊 |
| RequestPath | NVARCHAR(500) |  | Y | 請求路徑 |
| RequestBody | NVARCHAR(MAX) |  | Y | 請求內容 |
| UserId | BIGINT | FK | Y | 使用者 ID |
| IpAddress | NVARCHAR(50) |  | Y | IP 位址 |
| CreatedAt | DATETIME2 | IDX | N | 建立時間 |

---

## 5. 稽核紀錄的設計差異

| 資料表 | 用途 | 適合記錄 |
|---|---|---|
| AuditLogs | 資料層級變更 | 某欄位從 A 改成 B |
| AdminActionLogs | 業務操作層級 | 管理員執行商品上架 |
| SystemErrorLogs | 系統錯誤 | API 發生例外錯誤 |
| UserLoginLogs | 登入行為 | 登入成功、登入失敗 |

---

## 6. 稽核流程範例：修改商品價格

```text
後台管理員登入
    ↓
進入商品管理頁面
    ↓
修改商品 SKU 價格
    ↓
後端更新 ProductSkus
    ↓
建立 AuditLogs
    ↓
建立 AdminActionLogs
```

AuditLogs 範例：

```json
OldValueJson: { "SalePrice": 399 }
NewValueJson: { "SalePrice": 450 }
```

AdminActionLogs 範例：

```text
ModuleName: Product
ActionName: UpdatePrice
Description: 管理員修改 SKU 售價，由 399 調整為 450
```

---

## 7. 後台功能對應

| 後台功能 | 主要資料表 |
|---|---|
| 操作紀錄查詢 | AdminActionLogs |
| 資料異動查詢 | AuditLogs |
| 登入紀錄查詢 | UserLoginLogs |
| 系統錯誤查詢 | SystemErrorLogs |
| 安全事件追蹤 | UserLoginLogs, AdminActionLogs |

---

## 8. 本模組總結

稽核模組不是可有可無的附加功能，而是後台系統安全與管理的重要依據。只要涉及金額、庫存、訂單、退款與權限異動，都應留下操作紀錄，避免後續發生爭議時無法追蹤。


---


# 09 PK、FK、UK、CK、IDX、TK 設計說明

## 1. PK：Primary Key 主鍵

PK 用於唯一識別資料表中的每一筆資料。

範例：

```sql
UserId BIGINT IDENTITY(1,1) PRIMARY KEY
ProductId BIGINT IDENTITY(1,1) PRIMARY KEY
OrderId BIGINT IDENTITY(1,1) PRIMARY KEY
```

建議：

1. 系統內部主鍵使用整數流水號，例如 BIGINT IDENTITY。
2. 不要使用會變動的資料作為 PK，例如 Email、Phone。
3. 訂單編號 `OrderNo` 不建議當 PK，應作為 UK/TK。

---

## 2. FK：Foreign Key 外鍵

FK 用於建立資料表之間的關聯，避免產生孤兒資料。

範例：

```text
Orders.UserId -> Users.UserId
OrderItems.OrderId -> Orders.OrderId
OrderItems.SkuId -> ProductSkus.SkuId
```

SQL 範例：

```sql
ALTER TABLE Orders
ADD CONSTRAINT FK_Orders_Users
FOREIGN KEY (UserId) REFERENCES Users(UserId);
```

建議：

1. 訂單明細一定要關聯訂單主檔。
2. SKU 一定要關聯商品主檔。
3. 庫存一定要關聯 SKU 與倉庫。
4. 重要交易資料不建議使用 ON DELETE CASCADE。

---

## 3. UK：Unique Key 唯一鍵

UK 用於限制欄位不可重複。

常見使用情境：

| 欄位 | 說明 |
|---|---|
| Users.Account | 帳號不可重複 |
| Users.Email | Email 不可重複 |
| Products.ProductNo | 商品編號不可重複 |
| ProductSkus.SkuNo | SKU 編號不可重複 |
| Orders.OrderNo | 訂單編號不可重複 |
| Coupons.CouponCode | 優惠碼不可重複 |

SQL 範例：

```sql
ALTER TABLE Users
ADD CONSTRAINT UK_Users_Account UNIQUE (Account);
```

---

## 4. CK：Check Constraint 檢查限制

CK 用於限制欄位值必須符合合理範圍。

### 4.1 金額不可小於 0

```sql
ALTER TABLE ProductSkus
ADD CONSTRAINT CK_ProductSkus_SalePrice
CHECK (SalePrice >= 0);
```

### 4.2 數量不可小於 0

```sql
ALTER TABLE InventoryStocks
ADD CONSTRAINT CK_InventoryStocks_OnHandQty
CHECK (OnHandQty >= 0);
```

### 4.3 狀態限制

```sql
ALTER TABLE Orders
ADD CONSTRAINT CK_Orders_OrderStatus
CHECK (OrderStatus IN ('Pending', 'Paid', 'Processing', 'Shipped', 'Completed', 'Cancelled'));
```

---

## 5. IDX：Index 索引

Index 用於提升查詢速度。

### 5.1 常見索引欄位

| 資料表 | 欄位 | 原因 |
|---|---|---|
| Users | Account | 登入查詢 |
| Users | Email | 會員查詢 |
| Products | ProductName | 商品搜尋 |
| ProductSkus | SkuNo | SKU 查詢 |
| Orders | OrderNo | 訂單查詢 |
| Orders | UserId | 查詢會員訂單 |
| Orders | OrderedAt | 訂單日期區間查詢 |
| Payments | PaymentNo | 付款查詢 |
| Shipments | TrackingNo | 物流查詢 |
| InventoryTransactions | CreatedAt | 庫存異動查詢 |

### 5.2 複合索引範例

```sql
CREATE INDEX IX_Orders_UserId_OrderedAt
ON Orders(UserId, OrderedAt DESC);
```

適合情境：查詢某位會員最近的訂單。

---

## 6. TK：Tracking Key / Transaction Key 追蹤鍵

TK 不是 SQL Server 的正式約束，而是系統設計中常見的業務追蹤編號。

### 6.1 常見 TK

| 欄位 | 範例 | 用途 |
|---|---|---|
| UserNo | USR202605170001 | 使用者編號 |
| ProductNo | PRD202605170001 | 商品編號 |
| SkuNo | SKU202605170001 | SKU 編號 |
| OrderNo | ORD202605170001 | 訂單編號 |
| PaymentNo | PAY202605170001 | 付款編號 |
| ShipmentNo | SHP202605170001 | 出貨編號 |
| RefundNo | REF202605170001 | 退款編號 |
| TransactionNo | INV202605170001 | 庫存異動單號 |

### 6.2 TK 設計原則

1. 對外顯示使用 TK，不直接顯示 PK。
2. TK 必須唯一，因此通常搭配 UK。
3. TK 可包含日期，方便人工辨識。
4. TK 不應作為資料表關聯的 FK，FK 應使用內部 ID。

---

## 7. 命名規則建議

### 7.1 主鍵

```text
PK_資料表名稱
```

範例：

```text
PK_Users
PK_Orders
```

### 7.2 外鍵

```text
FK_子表_父表
```

範例：

```text
FK_Orders_Users
FK_OrderItems_Orders
FK_OrderItems_ProductSkus
```

### 7.3 唯一鍵

```text
UK_資料表_欄位
```

範例：

```text
UK_Users_Account
UK_Orders_OrderNo
```

### 7.4 索引

```text
IX_資料表_欄位
```

範例：

```text
IX_Orders_UserId_OrderedAt
```

---

## 8. 不建議的設計

### 8.1 不建議用商品名稱當關聯

錯誤：

```text
OrderItems.ProductName -> Products.ProductName
```

原因：商品名稱可能修改。

正確：

```text
OrderItems.ProductId -> Products.ProductId
```

---

### 8.2 不建議訂單直接刪除

訂單涉及付款、出貨與會計紀錄，不應直接刪除。

正確做法：

```text
OrderStatus = Cancelled
CancelledAt = GETDATE()
```

---

### 8.3 不建議用 FLOAT 存金額

錯誤：

```sql
Price FLOAT
```

正確：

```sql
Price DECIMAL(18,2)
```

---

## 9. 本章總結

PK、FK、UK、CK、IDX 與 TK 的設計會直接影響資料庫的穩定性、可維護性與查詢效能。電商系統資料量會逐漸累積，因此一開始就要建立清楚的主鍵、外鍵、唯一鍵、檢查限制與索引策略。


---


# 10 MSSQL 電商後台資料庫完整實作計畫

## 1. 實作總目標

本實作計畫的目標是建立一套可支援電商後台開發的 MSSQL 資料庫，並能與 ASP.NET Core 後端及前端管理頁面串接。

最終成果應包含：

1. 完整 MSSQL 資料表。
2. PK、FK、UK、CK、Index 設計。
3. 測試資料。
4. 後端 API 對應規劃。
5. 權限控管資料。
6. 訂單、付款、庫存交易流程。
7. 稽核與操作紀錄。
8. 部署與維護檢查清單。

---

## 2. 階段一：需求確認與資料範圍盤點

### 2.1 工作目標

確認電商後台需要管理哪些資料，避免資料表設計過度簡化。

### 2.2 工作細則

1. 確認商品是否有多規格。
2. 確認是否需要多倉庫。
3. 確認會員是否需要分級。
4. 確認是否需要優惠券。
5. 確認是否需要串接第三方金流。
6. 確認是否需要物流追蹤。
7. 確認後台人員是否需要權限控管。
8. 確認是否需要操作紀錄。

### 2.3 產出文件

```text
需求盤點表
資料模組清單
功能對應資料表清單
```

---

## 3. 階段二：資料庫命名與基本規範制定

### 3.1 工作目標

建立一致的資料表、欄位、主鍵與外鍵命名規範。

### 3.2 工作細則

1. 決定資料庫名稱，例如 EcommerceBackendDB。
2. 決定資料表命名方式，採用英文複數名詞。
3. 決定主鍵命名方式，例如 UserId、ProductId。
4. 決定業務編號命名方式，例如 OrderNo、PaymentNo。
5. 決定共用欄位，例如 CreatedAt、UpdatedAt、IsDeleted。
6. 決定金額欄位型別，統一使用 DECIMAL(18,2)。
7. 決定狀態欄位型別，初期可使用 NVARCHAR。

### 3.3 產出文件

```text
資料庫命名規範
欄位型別規範
狀態碼規範
```

---

## 4. 階段三：帳戶與權限資料表建置

### 4.1 工作目標

建立登入、角色與權限控管所需資料表。

### 4.2 建置資料表

```text
Users
Roles
UserRoles
Permissions
RolePermissions
UserLoginLogs
```

### 4.3 工作細則

1. 建立 Users 資料表。
2. 建立 Roles 資料表。
3. 建立 UserRoles 多對多關聯表。
4. 建立 Permissions 資料表。
5. 建立 RolePermissions 多對多關聯表。
6. 建立 UserLoginLogs 登入紀錄表。
7. 建立預設角色。
8. 建立預設權限。
9. 建立測試管理員帳號。

### 4.4 驗收條件

1. 管理員帳號可建立。
2. 一位使用者可擁有多個角色。
3. 一個角色可擁有多個權限。
4. 權限不可重複指派。
5. 登入紀錄可保存。

---

## 5. 階段四：商品模組建置

### 5.1 工作目標

建立商品分類、商品主檔、SKU、圖片與規格資料表。

### 5.2 建置資料表

```text
ProductCategories
Products
ProductSkus
ProductImages
ProductAttributes
ProductAttributeValues
```

### 5.3 工作細則

1. 建立商品分類表，支援父子分類。
2. 建立商品主檔，保存商品名稱、品牌、描述與上下架狀態。
3. 建立 SKU 表，保存價格、條碼、規格與銷售狀態。
4. 建立圖片表，支援多圖與主圖。
5. 建立商品屬性表，例如顏色、尺寸。
6. 建立 SKU 屬性值表。
7. 建立商品搜尋索引。
8. 建立測試商品資料。

### 5.4 驗收條件

1. 商品可建立多個 SKU。
2. SKU 可設定不同價格。
3. 商品可上架與下架。
4. 商品可設定多張圖片。
5. 商品分類可支援多層級。

---

## 6. 階段五：庫存模組建置

### 6.1 工作目標

建立倉庫、庫存現況、庫存異動與保留庫存資料表。

### 6.2 建置資料表

```text
Warehouses
InventoryStocks
InventoryTransactions
InventoryReservations
```

### 6.3 工作細則

1. 建立倉庫表。
2. 建立庫存現況表。
3. 建立庫存異動表。
4. 建立保留庫存表。
5. 建立庫存不可小於 0 的 CK。
6. 建立 WarehouseId + SkuId 唯一限制。
7. 建立庫存異動測試流程。

### 6.4 驗收條件

1. SKU 可在不同倉庫有不同庫存。
2. 下單時可保留庫存。
3. 取消訂單可釋放庫存。
4. 出貨可扣除庫存。
5. 每次庫存變更都有異動紀錄。

---

## 7. 階段六：購物車與訂單模組建置

### 7.1 工作目標

建立購物車、訂單主檔、訂單明細與訂單狀態歷史。

### 7.2 建置資料表

```text
ShoppingCarts
ShoppingCartItems
Orders
OrderItems
OrderStatusHistories
```

### 7.3 工作細則

1. 建立購物車主檔。
2. 建立購物車明細。
3. 建立訂單主檔。
4. 建立訂單明細。
5. 建立訂單狀態歷史表。
6. 設計訂單編號產生規則。
7. 建立訂單金額計算流程。
8. 建立訂單狀態轉換流程。

### 7.4 驗收條件

1. 購物車可轉成訂單。
2. 訂單明細可保存商品 Snapshot。
3. 訂單狀態可變更。
4. 每次狀態變更都有紀錄。
5. 訂單總金額計算正確。

---

## 8. 階段七：付款、物流與退款模組建置

### 8.1 工作目標

建立付款、金流交易、出貨、物流與退款資料表。

### 8.2 建置資料表

```text
Payments
PaymentTransactions
Shipments
ShipmentItems
Refunds
RefundItems
```

### 8.3 工作細則

1. 建立付款主檔。
2. 建立金流交易紀錄。
3. 建立出貨主檔。
4. 建立出貨明細。
5. 建立退款主檔。
6. 建立退款明細。
7. 設計付款狀態流程。
8. 設計出貨狀態流程。
9. 設計退款審核流程。

### 8.4 驗收條件

1. 一筆訂單可有付款紀錄。
2. 付款成功後訂單付款狀態可更新。
3. 一筆訂單可建立出貨紀錄。
4. 出貨後可扣除保留庫存。
5. 可建立退款申請與退款明細。

---

## 9. 階段八：優惠券與促銷模組建置

### 9.1 工作目標

建立優惠券、促銷活動、折扣紀錄與優惠使用紀錄。

### 9.2 建置資料表

```text
Coupons
CouponUsages
Promotions
PromotionProducts
OrderDiscounts
```

### 9.3 工作細則

1. 建立優惠券表。
2. 建立優惠券使用紀錄。
3. 建立促銷活動表。
4. 建立促銷商品關聯表。
5. 建立訂單折扣紀錄。
6. 設計優惠券驗證流程。
7. 設計折扣計算流程。
8. 測試滿額折扣與百分比折扣。

### 9.4 驗收條件

1. 優惠券可設定開始與結束時間。
2. 優惠券可限制總使用次數。
3. 優惠券可限制每人使用次數。
4. 訂單可保存實際折扣紀錄。
5. 促銷活動可指定商品。

---

## 10. 階段九：稽核與系統紀錄建置

### 10.1 工作目標

建立後台操作紀錄、資料異動紀錄與系統錯誤紀錄。

### 10.2 建置資料表

```text
AuditLogs
AdminActionLogs
SystemErrorLogs
```

### 10.3 工作細則

1. 建立 AuditLogs。
2. 建立 AdminActionLogs。
3. 建立 SystemErrorLogs。
4. 設計後端 Middleware 或 Service 記錄操作。
5. 設計資料異動前後 JSON 保存方式。
6. 設計後台查詢頁面。

### 10.4 驗收條件

1. 商品價格修改可留下紀錄。
2. 庫存調整可留下紀錄。
3. 退款核准可留下紀錄。
4. 系統錯誤可被記錄。
5. 登入失敗可被追蹤。

---

## 11. 階段十：後端 API 串接

### 11.1 工作目標

將資料表對應到 ASP.NET Core 後端 API。

### 11.2 工作細則

1. 建立 Entity Models。
2. 建立 DbContext。
3. 建立 DTO。
4. 建立 Repository。
5. 建立 Service。
6. 建立 Controller。
7. 建立 JWT 登入機制。
8. 建立權限驗證 Attribute。
9. 建立交易控制 TransactionScope。
10. 建立錯誤處理 Middleware。

### 11.3 建議 API 模組

```text
AuthController
UsersController
RolesController
ProductsController
InventoryController
OrdersController
PaymentsController
ShipmentsController
CouponsController
AuditLogsController
```

---

## 12. 階段十一：前端後台頁面串接

### 12.1 工作目標

建立後台管理頁面並串接 API。

### 12.2 工作細則

1. 建立登入頁面。
2. 建立後台 Layout。
3. 建立商品管理頁面。
4. 建立 SKU 管理頁面。
5. 建立庫存管理頁面。
6. 建立訂單管理頁面。
7. 建立付款與出貨頁面。
8. 建立優惠券管理頁面。
9. 建立權限管理頁面。
10. 建立操作紀錄查詢頁面。

---

## 13. 階段十二：測試與驗收

### 13.1 測試項目

1. 使用者登入測試。
2. 角色權限測試。
3. 商品 CRUD 測試。
4. SKU 價格測試。
5. 庫存異動測試。
6. 下單流程測試。
7. 付款流程測試。
8. 出貨扣庫測試。
9. 取消訂單釋放庫存測試。
10. 優惠券使用限制測試。
11. 退款流程測試。
12. 稽核紀錄測試。

---

## 14. 建議實作時程

| 週次 | 工作項目 |
|---|---|
| 第 1 週 | 需求盤點、資料庫命名規範、ERD 初稿 |
| 第 2 週 | 帳戶權限、商品模組資料表建置 |
| 第 3 週 | 庫存、購物車、訂單資料表建置 |
| 第 4 週 | 付款、物流、退款、優惠模組建置 |
| 第 5 週 | 稽核紀錄、索引、測試資料 |
| 第 6 週 | 後端 API 串接 |
| 第 7 週 | 前端後台頁面串接 |
| 第 8 週 | 測試、修正、部署 |

---

## 15. 實作總結

完整電商後台資料庫不只是商品與訂單兩張表，而是一套包含帳戶、商品、庫存、訂單、付款、物流、優惠與稽核的完整資料結構。建議依照模組分階段建置，每完成一個模組就進行測試，避免最後整合時才發現資料關聯錯誤。


---


# 12 ERD 文字版關聯整理

## 1. 帳戶與權限模組

```text
Users 1 ── N UserRoles
Roles 1 ── N UserRoles
Roles 1 ── N RolePermissions
Permissions 1 ── N RolePermissions
Users 1 ── N UserLoginLogs
```

說明：

- 使用者與角色為多對多關係。
- 角色與權限為多對多關係。
- 使用者登入會產生多筆登入紀錄。

---

## 2. 商品模組

```text
ProductCategories 1 ── N Products
ProductCategories 1 ── N ProductCategories
Products 1 ── N ProductSkus
Products 1 ── N ProductImages
ProductSkus 1 ── N ProductImages
ProductSkus 1 ── N ProductAttributeValues
ProductAttributes 1 ── N ProductAttributeValues
```

說明：

- 商品分類支援父子分類。
- 一個商品可以有多個 SKU。
- 一個商品可以有多張圖片。
- 一個 SKU 可以有多個屬性值。

---

## 3. 庫存模組

```text
Warehouses 1 ── N InventoryStocks
ProductSkus 1 ── N InventoryStocks
Warehouses 1 ── N InventoryTransactions
ProductSkus 1 ── N InventoryTransactions
Orders 1 ── N InventoryReservations
OrderItems 1 ── N InventoryReservations
ProductSkus 1 ── N InventoryReservations
```

說明：

- 一個 SKU 可以存在多個倉庫。
- 每次庫存變化都應建立異動紀錄。
- 訂單成立時可建立保留庫存。

---

## 4. 購物車與訂單模組

```text
Users 1 ── N ShoppingCarts
ShoppingCarts 1 ── N ShoppingCartItems
ProductSkus 1 ── N ShoppingCartItems
Users 1 ── N Orders
Orders 1 ── N OrderItems
Products 1 ── N OrderItems
ProductSkus 1 ── N OrderItems
Orders 1 ── N OrderStatusHistories
```

說明：

- 一個使用者可以有多筆購物車紀錄。
- 購物車明細會對應 SKU。
- 訂單主檔保存總金額與收件資訊。
- 訂單明細保存商品 Snapshot。

---

## 5. 付款、物流與退款模組

```text
Orders 1 ── N Payments
Payments 1 ── N PaymentTransactions
Orders 1 ── N Shipments
Shipments 1 ── N ShipmentItems
OrderItems 1 ── N ShipmentItems
Orders 1 ── N Refunds
Payments 1 ── N Refunds
Refunds 1 ── N RefundItems
OrderItems 1 ── N RefundItems
```

說明：

- 一筆訂單可有多次付款嘗試。
- 一筆付款可有多筆金流交易紀錄。
- 一筆訂單可分批出貨。
- 一筆訂單可部分退款。

---

## 6. 優惠模組

```text
Coupons 1 ── N CouponUsages
Users 1 ── N CouponUsages
Orders 1 ── N CouponUsages
Promotions 1 ── N PromotionProducts
Products 1 ── N PromotionProducts
ProductSkus 1 ── N PromotionProducts
Orders 1 ── N OrderDiscounts
Coupons 1 ── N OrderDiscounts
Promotions 1 ── N OrderDiscounts
```

說明：

- 優惠券使用後需保存使用紀錄。
- 促銷活動可指定商品或 SKU。
- 訂單需保存實際折扣紀錄。

---

## 7. 稽核模組

```text
Users 1 ── N AuditLogs
Users 1 ── N AdminActionLogs
Users 1 ── N SystemErrorLogs
```

說明：

- 使用者操作資料時建立 AuditLogs。
- 後台功能操作建立 AdminActionLogs。
- 系統錯誤建立 SystemErrorLogs。

---

## 8. 簡化整體 ERD

```text
Users
 ├─ UserRoles ─ Roles ─ RolePermissions ─ Permissions
 ├─ ShoppingCarts ─ ShoppingCartItems ─ ProductSkus
 ├─ Orders ─ OrderItems ─ ProductSkus ─ Products ─ ProductCategories
 ├─ CouponUsages ─ Coupons
 └─ AdminActionLogs / AuditLogs / UserLoginLogs

Products
 ├─ ProductSkus
 │   ├─ InventoryStocks ─ Warehouses
 │   ├─ InventoryTransactions
 │   ├─ ProductAttributeValues ─ ProductAttributes
 │   └─ OrderItems
 └─ ProductImages

Orders
 ├─ OrderItems
 ├─ Payments ─ PaymentTransactions
 ├─ Shipments ─ ShipmentItems
 ├─ Refunds ─ RefundItems
 ├─ OrderDiscounts
 ├─ OrderStatusHistories
 └─ InventoryReservations
```

---

## 9. 關聯設計重點

1. 所有交易主檔都使用系統內部 ID 作為 FK。
2. 所有對外顯示的編號使用 TK，例如 OrderNo、PaymentNo。
3. 訂單明細保存商品 Snapshot，避免歷史資料被商品主檔修改影響。
4. 庫存現況與庫存異動分離。
5. 優惠規則與實際折扣紀錄分離。
6. 稽核紀錄獨立保存，不依賴單一模組。


---


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


---


# 14 安全性、交易一致性與資料完整性建議

## 1. 密碼安全

使用者密碼不可明碼保存。

建議：

1. 使用 ASP.NET Core Identity 的 PasswordHasher。
2. 或使用 BCrypt / PBKDF2 / Argon2。
3. 密碼欄位只保存 Hash。
4. 登入失敗多次需鎖定帳號。
5. 後台管理員建議啟用 MFA。

---

## 2. 權限安全

後台 API 不應只靠前端選單隱藏控制權限。

正確做法：

```text
前端：根據權限隱藏按鈕
後端：每支 API 再次驗證 PermissionCode
資料庫：記錄每次重要操作
```

---

## 3. SQL Injection 防護

若使用 EF Core，請避免拼接 SQL 字串。

不建議：

```csharp
var sql = "SELECT * FROM Users WHERE Account = '" + account + "'";
```

建議：

```csharp
var user = await db.Users.FirstOrDefaultAsync(x => x.Account == account);
```

若使用 Dapper，請使用參數化查詢。

---

## 4. 交易一致性

電商系統常見問題是訂單成功但庫存未扣、付款成功但訂單狀態未更新。這類流程必須使用交易控制。

### 4.1 建立訂單交易

```text
Begin Transaction
    建立 Orders
    建立 OrderItems
    檢查庫存
    保留庫存
    建立 InventoryReservations
    建立 InventoryTransactions
Commit
```

若其中一步失敗，全部 Rollback。

---

## 5. 避免超賣設計

下單時不可只在前端檢查庫存，後端也要檢查。

建議流程：

```text
查詢 AvailableQty
    ↓
若不足，回傳庫存不足
    ↓
若足夠，於 Transaction 中更新 ReservedQty
    ↓
建立保留庫存紀錄
```

高併發時建議搭配：

1. Transaction。
2. RowVersion。
3. SQL Update 條件檢查。
4. Redis 分散式鎖，若系統規模較大。

---

## 6. 金額計算一致性

金額計算應在後端完成，不應相信前端傳來的總金額。

前端可以傳：

```text
SkuId
Quantity
CouponCode
ShippingMethod
```

後端應自行計算：

```text
商品單價
商品小計
優惠折扣
運費
訂單總額
```

---

## 7. 訂單不可直接刪除

訂單與付款、退款、出貨、庫存異動有關，不可直接刪除。

應使用狀態欄位：

```text
OrderStatus = Cancelled
CancelledAt = GETDATE()
```

---

## 8. 商品刪除建議使用軟刪除

商品可能已存在歷史訂單中，因此不建議直接刪除。

建議：

```text
IsDeleted = 1
Status = Archived
```

---

## 9. 個資保護

會員資料包含 Email、Phone、Address，應注意：

1. 後台只顯示必要資訊。
2. 操作會員資料需記錄 AdminActionLogs。
3. 敏感欄位可考慮加密。
4. 資料匯出需權限控管。
5. 不要在 SystemErrorLogs 中保存完整密碼或信用卡資料。

---

## 10. 金流資料安全

不建議保存完整信用卡號。

可保存：

```text
付款方式
金流商交易編號
授權碼
付款狀態
付款時間
```

不可保存：

```text
完整信用卡號
CVV
未加密卡片資訊
```

---

## 11. 稽核紀錄不可任意刪除

稽核紀錄用於追蹤責任，不建議提供一般後台人員刪除功能。

建議：

1. 只允許查詢。
2. 不允許修改。
3. 不允許刪除。
4. 需要資料保存期限時，透過批次封存。

---

## 12. 資料完整性檢查

建議定期檢查：

1. 是否有訂單沒有訂單明細。
2. 是否有付款成功但訂單仍 Pending。
3. 是否有庫存保留但訂單已取消。
4. 是否有 Shipment 已出貨但庫存未扣。
5. 是否有優惠券 UsedCount 與 CouponUsages 筆數不一致。

---

## 13. 本章總結

電商系統的安全與資料一致性非常重要。後端必須負責金額計算、權限檢查、交易控制與稽核紀錄，前端只負責操作介面與資料呈現。資料庫則透過 PK、FK、UK、CK 與 Transaction 保護資料完整性。


---


# 15 MSSQL 電商後台資料庫部署檢查清單

## 1. 部署前檢查

| 檢查項目 | 是否完成 |
|---|---|
| 資料庫名稱已確認 | □ |
| 資料表命名規範已確認 | □ |
| PK 設計完成 | □ |
| FK 設計完成 | □ |
| UK 設計完成 | □ |
| CK 設計完成 | □ |
| Index 設計完成 | □ |
| 測試資料準備完成 | □ |
| 備份策略確認 | □ |
| 使用者權限確認 | □ |

---

## 2. 資料庫建置檢查

| 檢查項目 | 是否完成 |
|---|---|
| 建立 Database | □ |
| 建立 Schema | □ |
| 建立帳戶權限資料表 | □ |
| 建立商品資料表 | □ |
| 建立庫存資料表 | □ |
| 建立訂單資料表 | □ |
| 建立付款物流退款資料表 | □ |
| 建立優惠資料表 | □ |
| 建立稽核紀錄資料表 | □ |
| 建立索引 | □ |

---

## 3. 測試資料檢查

| 測試資料 | 是否完成 |
|---|---|
| 預設管理員帳號 | □ |
| 預設角色 | □ |
| 預設權限 | □ |
| 商品分類 | □ |
| 測試商品 | □ |
| 測試 SKU | □ |
| 測試倉庫 | □ |
| 測試庫存 | □ |
| 測試優惠券 | □ |

---

## 4. 功能測試檢查

| 測試項目 | 是否通過 |
|---|---|
| 使用者登入 | □ |
| 權限驗證 | □ |
| 商品新增 | □ |
| 商品修改 | □ |
| SKU 新增 | □ |
| 庫存調整 | □ |
| 加入購物車 | □ |
| 建立訂單 | □ |
| 保留庫存 | □ |
| 付款成功 | □ |
| 出貨扣庫 | □ |
| 取消訂單釋放庫存 | □ |
| 優惠券使用 | □ |
| 退款流程 | □ |
| 稽核紀錄 | □ |

---

## 5. 效能檢查

| 檢查項目 | 是否完成 |
|---|---|
| 訂單列表查詢有索引 | □ |
| 商品搜尋有索引 | □ |
| SKU 查詢有索引 | □ |
| 會員訂單查詢有索引 | □ |
| 庫存異動查詢有索引 | □ |
| 登入帳號查詢有索引 | □ |
| 慢查詢已檢查 | □ |

---

## 6. 安全檢查

| 檢查項目 | 是否完成 |
|---|---|
| 密碼未明碼保存 | □ |
| 後台 API 有權限驗證 | □ |
| SQL 查詢使用參數化 | □ |
| 金流資料未保存敏感卡號 | □ |
| 操作紀錄有保存 | □ |
| 登入失敗有紀錄 | □ |
| 錯誤紀錄未暴露敏感資料 | □ |
| 資料庫帳號權限最小化 | □ |

---

## 7. 備份與維護檢查

| 檢查項目 | 是否完成 |
|---|---|
| 每日自動備份 | □ |
| 備份還原測試 | □ |
| 交易紀錄備份 | □ |
| 索引重建計畫 | □ |
| 資料庫容量監控 | □ |
| 錯誤紀錄監控 | □ |
| 稽核紀錄封存策略 | □ |

---

## 8. 上線前最終檢查

1. 確認正式資料庫連線字串。
2. 確認正式環境帳號密碼。
3. 確認測試資料不會誤留在正式環境。
4. 確認後台管理員帳號已建立。
5. 確認 SSL / HTTPS。
6. 確認金流測試環境與正式環境切換。
7. 確認物流 API 設定。
8. 確認錯誤通知機制。
9. 確認資料庫備份已啟用。
10. 確認上線回復方案。

---

## 9. 部署後檢查

| 檢查項目 | 是否完成 |
|---|---|
| 登入功能正常 | □ |
| 商品列表正常 | □ |
| 商品詳細正常 | □ |
| 購物車正常 | □ |
| 建立訂單正常 | □ |
| 付款流程正常 | □ |
| 出貨流程正常 | □ |
| 後台操作紀錄正常 | □ |
| 系統錯誤紀錄正常 | □ |
| 備份任務正常 | □ |

---

## 10. 本章總結

資料庫部署不只是執行建表 SQL，還要確認資料完整性、安全性、效能、備份與後端 API 串接。建議每次部署都保留檢查清單，方便日後維護與交接。


---


# MSSQL 電商後台資料庫完整設計文件

本文件提供一套適合全端開發專案使用的 MSSQL 電商後台資料庫設計，適用於 ASP.NET Core MVC / Web API / Vue / React / Angular 等前後端分離架構。資料庫設計涵蓋：使用者帳戶、角色權限、商品品項、SKU、庫存、購物車、訂單、付款、物流、優惠券、退款、後台操作紀錄與資料稽核。

---

## 文件目錄

| 檔案 | 說明 |
|---|---|
| `01_資料庫系統範圍與設計原則.md` | 系統範圍、資料庫命名原則、資料分層概念 |
| `02_帳戶角色與權限資料表設計.md` | 使用者、角色、權限、登入紀錄設計 |
| `03_商品分類SKU與規格資料表設計.md` | 商品、分類、SKU、圖片、規格設計 |
| `04_庫存倉庫與庫存異動資料表設計.md` | 庫存、倉庫、庫存異動、保留庫存設計 |
| `05_購物車訂單與訂單明細資料表設計.md` | 購物車、訂單主檔、訂單明細設計 |
| `06_付款物流與退款資料表設計.md` | 付款、物流、退款設計 |
| `07_優惠券促銷與折扣資料表設計.md` | 優惠券、促銷活動、折扣紀錄設計 |
| `08_後台操作紀錄稽核與系統紀錄設計.md` | 後台操作紀錄、登入紀錄、資料異動追蹤 |
| `09_主鍵外鍵唯一鍵檢查限制索引與追蹤鍵設計說明.md` | PK、FK、UK、CK、Index、TK 設計說明 |
| `10_MSSQL電商後台資料庫完整實作計畫.md` | 完整實作計畫與工作細則 |
| `11_mssql_create_table_script.sql` | MSSQL 建表 SQL 範例 |
| `12_ERD文字版資料表關聯整理.md` | ERD 文字版資料表關聯整理 |
| `13_後端API與資料表對應規劃.md` | 後端 API 與資料表對應規劃 |
| `14_安全性交易一致性與資料完整性建議.md` | 安全性、交易一致性、資料完整性建議 |
| `15_MSSQL電商後台資料庫部署檢查清單.md` | 資料庫建置、測試、部署檢查清單 |

---

## 資料庫設計目標

本資料庫設計目標不是只有建立幾張商品與訂單表，而是建立一套可擴充的電商後台資料結構，能支援後續系統開發、API 串接、權限控管、報表查詢與資料稽核。

主要目標如下：

1. **支援商品管理**  
   可管理商品分類、商品主檔、SKU、商品圖片、規格屬性與上下架狀態。

2. **支援帳號與權限控管**  
   可管理前台會員與後台管理員，並透過角色與權限控制不同功能的存取範圍。

3. **支援訂單流程**  
   從購物車、成立訂單、付款、出貨、取消、退款到完成訂單，皆有資料表可對應。

4. **支援庫存控管**  
   可記錄商品 SKU 庫存、倉庫、庫存異動、保留庫存與扣庫存流程。

5. **支援優惠活動**  
   可建立優惠券、促銷活動、折扣條件、使用限制與訂單折扣紀錄。

6. **支援後台稽核**  
   可記錄後台人員登入、操作、資料新增、修改、刪除與異常行為。

7. **支援全端開發擴充**  
   資料表可對應 ASP.NET Core Web API、Entity Framework Core、Dapper、Vue 前端頁面與後台管理功能。

---

## 建議資料庫命名

```sql
CREATE DATABASE EcommerceBackendDB;
```

建議資料表採用清楚且具語意的命名，例如：

```text
Users
Roles
Permissions
Products
ProductCategories
ProductSkus
InventoryStocks
Orders
OrderItems
Payments
Shipments
Coupons
AuditLogs
```

---

## 整體資料模組

```text
帳戶權限模組
├─ Users
├─ Roles
├─ UserRoles
├─ Permissions
├─ RolePermissions
└─ UserLoginLogs

商品模組
├─ ProductCategories
├─ Products
├─ ProductSkus
├─ ProductImages
├─ ProductAttributes
└─ ProductAttributeValues

庫存模組
├─ Warehouses
├─ InventoryStocks
├─ InventoryTransactions
└─ InventoryReservations

購物車與訂單模組
├─ ShoppingCarts
├─ ShoppingCartItems
├─ Orders
├─ OrderItems
└─ OrderStatusHistories

付款物流退款模組
├─ Payments
├─ PaymentTransactions
├─ Shipments
├─ ShipmentItems
├─ Refunds
└─ RefundItems

優惠模組
├─ Coupons
├─ CouponUsages
├─ Promotions
├─ PromotionProducts
└─ OrderDiscounts

稽核模組
├─ AuditLogs
├─ AdminActionLogs
└─ SystemErrorLogs
```

---

## 建議實作順序

1. 建立資料庫與基礎 Schema。
2. 建立帳號、角色、權限相關資料表。
3. 建立商品分類、商品主檔、SKU 與圖片資料表。
4. 建立倉庫、庫存與庫存異動資料表。
5. 建立購物車、訂單與訂單明細資料表。
6. 建立付款、物流與退款資料表。
7. 建立優惠券、促銷與折扣資料表。
8. 建立稽核紀錄與登入紀錄資料表。
9. 建立 Index、Constraint、View、Stored Procedure。
10. 匯入測試資料並進行 CRUD 測試。
11. 串接後端 API。
12. 串接前端管理頁面。
13. 進行壓力測試、交易測試與權限測試。
14. 部署正式資料庫。

---

## 關於 PK、FK、UK、CK、TK 的簡要說明

| 縮寫 | 名稱 | 用途 |
|---|---|---|
| PK | Primary Key | 主鍵，用來唯一識別每一筆資料 |
| FK | Foreign Key | 外鍵，用來建立資料表之間的關聯 |
| UK | Unique Key | 唯一鍵，用來限制欄位不可重複 |
| CK | Check Constraint | 檢查限制，用來限制欄位值的合理範圍 |
| IDX | Index | 索引，用來提升查詢速度 |
| TK | Tracking Key / Transaction Key | 追蹤鍵或交易鍵，常用於訂單編號、付款編號、物流追蹤碼、庫存異動單號 |

> 注意：TK 不是 SQL Server 標準約束名稱，而是系統設計中常用的「可追蹤業務編號」。例如 OrderNo、PaymentNo、ShipmentNo、InventoryTransactionNo。

---

## 適合的專案架構

此資料庫可搭配以下架構：

```text
Frontend
├─ Vue / React / Angular
├─ 商品管理頁面
├─ 訂單管理頁面
├─ 會員管理頁面
├─ 優惠券管理頁面
└─ 報表儀表板

Backend
├─ ASP.NET Core MVC / Web API
├─ Controllers
├─ Services
├─ Repositories
├─ DTOs
├─ EF Core / Dapper
└─ Authentication / Authorization

Database
├─ MSSQL
├─ Tables
├─ Views
├─ Stored Procedures
├─ Indexes
└─ Transaction Control
```
