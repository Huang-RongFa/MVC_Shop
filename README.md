# MyMVC 電商系統

## 專案簡介

本專案為完整電商網站系統，包含前台購物網站、後台管理系統、ASP.NET Core Web API、EF Core 與 MSSQL Database。

本專案不是教學型單表 CRUD，而是以正式商業系統為目標進行設計。

系統包含：

* Storefront App：前台顧客使用
* Admin App：後台管理員與員工使用
* ASP.NET Core Web API：前台與後台共用後端
* MSSQL Database：共用資料庫
* EF Core：資料存取
* Cookie Authentication：正式登入狀態
* Role / Permission：後台權限控管
* Audit Logs：重要操作紀錄

---

## 系統架構

本專案採用：

```text
ASP.NET Core Web API 模組化單體
+
共用 MSSQL Database
+
Storefront Vue App
+
Admin Vue App
```

整體資料流：

```text
Storefront App
  ↓
前台 API
  ↓
ASP.NET Core Web API
  ↓
Service
  ↓
Repository
  ↓
EF Core
  ↓
MSSQL Database

Admin App
  ↓
後台 API
  ↓
ASP.NET Core Web API
  ↓
Service
  ↓
Repository
  ↓
EF Core
  ↓
MSSQL Database
```

---

## 技術棧

### Backend

* C#
* ASP.NET Core Web API
* ASP.NET Core MVC
* EF Core
* MSSQL
* Cookie Authentication
* Role / Permission Authorization

### Frontend

* Vue
* Vue Router
* Pinia
* Axios 或 Fetch
* Bootstrap 5
* SweetAlert2
* RWD

### Database

* Microsoft SQL Server
* MSSQL Create Table Script
* ERD 文件
* 模組化資料表設計

---

## 文件導覽

開發前請優先閱讀：

```text
CONTEXT.md
```

接著依照任務閱讀下列文件。

### 架構決策

```text
docs/adr/
```

用途：

* 記錄已確認的架構決策
* 避免開發中出現互相矛盾的做法
* 若新設計與 ADR 衝突，應先新增或修正 ADR

---

### 資料庫設計

```text
docs/MSSQL/
```

用途：

* 資料庫模組設計
* 資料表欄位
* PK / FK / Index / Check Constraint
* 建表 SQL
* ERD 關聯
* API 與資料表對應

後端開發時，Entity 與 DbContext 應以此資料夾內容為準。

---

### MVC / Web API 架構

```text
docs/MVC/
```

用途：

* ASP.NET Core Web API 架構規劃
* EF Core 對應規則
* Repository / Service 分層
* Program.cs 與 Middleware Pipeline
* Cookie Authentication
* RESTful API
* OWASP 資安檢查
* 完整開發流程
* 常用指令與模板

---

### 正式上線技術規範

```text
docs/正式上線技術規範/
```

用途：

* 正式環境設定
* HTTPS / DNS / TLS
* 環境分離
* 機密管理
* API 安全
* 監控、日誌、告警
* 前端正式上線規範

---

### 頁面設計

```text
docs/頁面設計/
```

用途：

* Storefront 頁面設計
* Admin 頁面設計
* 登入、註冊、忘記密碼
* 商品、購物車、訂單
* 後台商品、庫存、付款、物流、退款、優惠、稽核管理

---

## 建議專案結構

```text
MyMVC/
├─ backend/
│  ├─ MySystem.Api/
│  │  ├─ Controllers/
│  │  │  ├─ Storefront/
│  │  │  └─ Admin/
│  │  ├─ Middleware/
│  │  ├─ Extensions/
│  │  ├─ Program.cs
│  │  └─ appsettings.json
│  │
│  ├─ MySystem.Application/
│  │  ├─ Services/
│  │  ├─ Interfaces/
│  │  ├─ DTOs/
│  │  │  ├─ Storefront/
│  │  │  ├─ Admin/
│  │  │  └─ Common/
│  │  └─ Validators/
│  │
│  ├─ MySystem.Domain/
│  │  ├─ Entities/
│  │  ├─ Enums/
│  │  └─ BusinessRules/
│  │
│  └─ MySystem.Infrastructure/
│     ├─ Data/
│     ├─ Configurations/
│     ├─ Repositories/
│     └─ Migrations/
│
├─ frontend/
│  ├─ storefront/
│  └─ admin/
│
├─ docs/
│  ├─ adr/
│  ├─ MSSQL/
│  ├─ MVC/
│  ├─ 正式上線技術規範/
│  └─ 頁面設計/
│
├─ README.md
└─ CONTEXT.md
```

---

## 核心模組

### 1. 帳戶與權限

負責：

* 前台會員登入
* 後台管理員登入
* 使用者資料
* 角色
* 權限
* 登入紀錄

主要資料表：

* Users
* Roles
* UserRoles
* Permissions
* RolePermissions
* UserLoginLogs

---

### 2. 商品目錄

負責：

* 商品分類
* 商品資料
* SKU
* 商品圖片
* 商品規格

主要資料表：

* ProductCategories
* Products
* ProductSkus
* ProductImages
* ProductAttributes
* ProductAttributeValues

---

### 3. 庫存

負責：

* 倉庫
* 庫存量
* 庫存異動
* 保留庫存
* 出貨扣庫存

主要資料表：

* Warehouses
* InventoryStocks
* InventoryTransactions
* InventoryReservations

---

### 4. 購物車與訂單

負責：

* 購物車
* 建立訂單
* 訂單明細
* 訂單狀態歷史
* 取消訂單

主要資料表：

* ShoppingCarts
* ShoppingCartItems
* Orders
* OrderItems
* OrderStatusHistories

---

### 5. 付款、物流與退款

負責：

* 付款
* 金流交易紀錄
* 出貨
* 物流明細
* 整筆退款
* 部分退款

主要資料表：

* Payments
* PaymentTransactions
* Shipments
* ShipmentItems
* Refunds
* RefundItems

---

### 6. 優惠與促銷

負責：

* 優惠券
* 優惠券使用紀錄
* 促銷
* 促銷商品
* 訂單折扣紀錄

主要資料表：

* Coupons
* CouponUsages
* Promotions
* PromotionProducts
* OrderDiscounts

---

### 7. 稽核與系統紀錄

負責：

* 後台操作紀錄
* 資料異動紀錄
* 系統錯誤紀錄
* 登入紀錄

主要資料表：

* AuditLogs
* AdminActionLogs
* SystemErrorLogs
* UserLoginLogs

---

## 開發原則

請遵守：

* 小步修改
* 可測試
* 可回復
* 安全優先
* 不任意大規模重構
* 不直接修改資料庫設計
* 不把商業邏輯寫在 Controller
* 不把 Entity 直接回傳給 API
* 不把 Repository 寫成商業流程中心
* 不把權限判斷只放在前端
* 不把正式登入狀態設計成 localStorage 高權限 JWT

---

## 後端分層

```text
Controller
  ↓
Service
  ↓
Repository
  ↓
DbContext
  ↓
MSSQL
```

### Controller

負責：

* 接收 Request
* Model Validation
* 呼叫 Service
* 回傳 HTTP Status Code

不負責：

* 複雜商業邏輯
* 多資料表交易流程
* 直接操作 DbContext

---

### Service

負責：

* 商業邏輯
* 權限判斷
* 訂單流程
* 庫存流程
* 付款流程
* 退款流程
* DTO 組裝
* Transaction 控制

不負責：

* HTTP 細節

---

### Repository

負責：

* 查詢資料
* 新增資料
* 更新資料
* 刪除資料

不負責：

* 決定商業規則
* 決定訂單狀態流程
* 決定是否可以退款
* 決定是否出貨扣庫存

---

## 開發流程

建議順序：

1. 確認 MSSQL Database
2. 建立 ASP.NET Core Web API 專案
3. 建立 Entity / DbContext
4. 建立 Repository / Service 分層
5. 建立共用 ApiResponse / PagedResult
6. 建立 Cookie Authentication
7. 建立 Role / Permission
8. 建立商品目錄 API
9. 建立庫存 API
10. 建立購物車與訂單 API
11. 建立付款、物流、退款 API
12. 建立優惠與促銷 API
13. 建立稽核紀錄 API
14. 建立 Storefront App
15. 建立 Admin App
16. 進行 OWASP 資安檢查
17. 測試
18. 部署

---

## AI 開發注意事項

若使用 Codex、ChatGPT、Claude Code、Cursor 或其他 AI 工具，請務必先要求 AI 閱讀：

```text
README.md
CONTEXT.md
docs/adr/*
docs/MSSQL/*
docs/MVC/*
```

AI 產生程式碼前，必須先說明：

* 目的
* 會修改哪些檔案
* 屬於哪個分層
* 是否涉及權限
* 是否涉及 Transaction
* 是否涉及資安
* 測試方式

---

## 不應該做的事

請避免：

* Controller 直接操作 DbContext
* Controller 寫大量商業邏輯
* Entity 直接回傳前端
* 密碼明文儲存
* PasswordHash 傳給前端
* localStorage 長期保存高權限 Token
* AllowAnyOrigin 用於正式環境
* Swagger 直接暴露於正式環境
* 忽略 Transaction
* 忽略 Audit Log
* 忽略前台與後台 API 分界
* 把付款成功與出貨流程混成同一步

---

## 下一步

建議先完成：

1. 後端 Solution 與四層專案
2. Entity 與 DbContext 對應 MSSQL
3. AuthService 與 Cookie Authentication
4. 前台 / 後台 API Controller 邊界
5. 商品目錄 API
---

## 頁面設計與 Views 控管

本專案目前可先使用 ASP.NET Core MVC Razor Views 製作前台與後台頁面，並以 DayNight Admin 版型作為視覺基礎。

頁面設計文件位置：

```text
docs/頁面設計/
```

建議先閱讀：

```text
docs/頁面設計/README_頁面設計總覽.md
docs/頁面設計/01_前台與後台頁面邊界.md
docs/頁面設計/02_Views資料夾規劃.md
docs/頁面設計/12_頁面設計開發步驟總表.md
```

### 頁面設計分界

| 區域 | 用途 | 是否顯示數據分析 |
|---|---|---:|
| 前台 Storefront | 商品展示、購物車、結帳、我的訂單 | 否 |
| 後台 Admin | Dashboard、商品、庫存、訂單、付款、出貨、退款、優惠、稽核 | 是，依權限 |
| 帳號 Auth | 登入、註冊、忘記密碼 | 否 |

### DayNight 版型用途

| 版型檔案 | 專案用途 |
|---|---|
| `login.html` | 前台登入、後台登入、註冊、忘記密碼 |
| `index.html` | 後台 Dashboard |
| `analytics.html` | 後台數據分析 |
| `projects.html` | 後台商品 / 訂單 / 任務看板式管理 |
| `inbox.html` | 後台訊息、通知、客服訊息 |
| `settings.html` | 後台設定、會員資料設定 |
| `about-templatemo.html` | 前台關於我們、首頁商品展示區塊 |

### 頁面開發順序

```text
1. 搬移 CSS / JS 到 wwwroot
2. 建立 Storefront / Admin / Auth Layout
3. 建立前台公開頁：首頁、商品列表、商品詳細、關於我們
4. 建立帳號頁：登入、註冊、忘記密碼
5. 建立前台會員頁：購物車、結帳、我的訂單
6. 建立後台登入與 Dashboard
7. 建立後台商品、庫存、訂單、付款、出貨、退款、優惠、稽核頁
8. 移除假資料，改用 ViewModel
9. 加入權限、CSRF、稽核與正式上線驗收
```
