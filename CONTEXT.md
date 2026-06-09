# CONTEXT.md

## 這份文件的用途

本文件提供 AI coding agent 使用，包括：

* Codex
* ChatGPT
* Claude Code
* Cursor
* Windsurf
* 其他 AI 開發工具

AI 在修改本專案前，必須先閱讀本文件。

本文件的目的不是教學，而是讓 AI 對齊本專案的架構、術語、分層責任、資安規則與開發流程。

---

## 專案定位

本專案是一套完整電商平台。

目標：

* 可正式上線
* 可長期維護
* 可逐步擴充
* 可測試
* 可回復
* 資安優先

本專案不是：

* 教學型專案
* Demo 專案
* 單表 CRUD 專案
* 只做後台管理的系統
* 只做 MVC View 的傳統網站

---

## 核心架構

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

正式架構語言：

```text
Storefront App
  ↓
前台 API
  ↓
ASP.NET Core Web API
  ↓
Service / Repository / EF Core
  ↓
共用 MSSQL Database
  ↑
Service / Repository / EF Core
  ↑
ASP.NET Core Web API
  ↑
後台 API
  ↑
Admin App
```

---

## 已存在的文件

AI 開發時應依照任務讀取相關文件。

### 架構決策

```text
docs/adr/
```

用途：

* 記錄架構決策
* 若文件與 ADR 衝突，以 ADR 為優先
* 若要推翻既有決策，應先提出 ADR 修改建議

---

### 資料庫文件

```text
docs/MSSQL/
```

用途：

* 資料表設計
* 欄位設計
* 關聯設計
* 建表 SQL
* ERD
* API 與資料表對應

Entity、DbContext、Repository 應以此資料夾為資料庫真實來源。

---

### MVC / Web API 文件

```text
docs/MVC/
```

用途：

* 系統總覽
* EF Core 規劃
* 後端分層
* Repository / Service
* Program.cs
* Middleware
* Cookie Authentication
* RESTful API
* OWASP
* 開發流程
* 常用指令

---

### 正式上線規範

```text
docs/正式上線技術規範/
```

用途：

* 正式環境設定
* 網路與 TLS
* 環境分離
* 機密管理
* API 安全
* 監控與日誌
* 前端上線規範

---

### 頁面設計

```text
docs/頁面設計/
```

用途：

* Storefront 頁面規劃
* Admin 頁面規劃
* 登入與註冊頁
* 商品頁
* 購物車與訂單頁
* 後台管理頁

---

## 系統邊界

### Storefront App

前台顧客使用。

主要功能：

* 首頁
* 商品列表
* 商品詳細
* 商品搜尋
* 商品篩選
* 加入購物車
* 查看購物車
* 結帳
* 付款
* 會員登入
* 會員註冊
* 忘記密碼
* 我的訂單
* 訂單詳細

核心規則：

* 顧客只能操作自己的資料
* 前台不能傳入可信價格
* 前台不能決定訂單折扣結果
* 前台不能決定使用者角色或權限

---

### Admin App

後台管理員與員工使用。

主要功能：

* Dashboard
* 使用者管理
* 角色權限管理
* 商品管理
* SKU 管理
* 商品圖片管理
* 庫存查詢
* 庫存調整
* 訂單管理
* 付款管理
* 出貨管理
* 退款管理
* 優惠券管理
* 促銷管理
* 稽核紀錄查詢

核心規則：

* 後台 API 必須檢查 Role 或 Permission
* 隱藏按鈕只是 UX，不是安全控制
* 重要操作要寫入 AdminActionLogs 或 AuditLogs
* 權限異動、退款、庫存調整、訂單狀態修改必須嚴格檢查

---

## 後端分層

本專案固定採用：

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

---

## Controller 規則

Controller 負責：

* 接收 HTTP Request
* 基本 Model Validation
* 呼叫 Service
* 回傳 HTTP Status Code
* 回傳 DTO

Controller 禁止：

* 寫複雜商業流程
* 直接操作 DbContext
* 直接處理多表交易
* 直接回傳 Entity
* 決定庫存是否扣除
* 決定訂單是否可退款
* 決定付款成功後的複雜狀態流轉

Controller 命名建議：

```text
StorefrontProductsController
StorefrontCartController
StorefrontOrdersController
StorefrontPaymentsController

AdminProductsController
AdminInventoryController
AdminOrdersController
AdminPaymentsController
AdminShipmentsController
AdminRefundsController
AdminCouponsController
AdminAuditLogsController
```

---

## Service 規則

Service 是系統商業邏輯核心。

Service 負責：

* 商業規則
* 權限判斷
* 訂單流程
* 庫存流程
* 付款流程
* 出貨流程
* 退款流程
* 優惠券驗證
* DTO 組裝
* Transaction 控制
* 呼叫 Repository

Service 禁止：

* 直接處理 HTTP Response
* 回傳 ASP.NET Core IActionResult
* 依賴 Controller
* 把資料庫查詢細節大量散落在 Service 中

重要流程必須放在 Service：

* 登入
* 建立訂單
* 取消訂單
* 付款成功
* 建立出貨
* 確認出貨
* 建立退款
* 使用優惠券
* 寫入後台操作紀錄

---

## Repository 規則

Repository 負責資料存取。

Repository 可以：

* 查詢資料
* 新增資料
* 更新資料
* 刪除資料
* 組合必要的 EF Core 查詢
* 回傳 Entity 或查詢投影結果

Repository 禁止：

* 決定商業規則
* 決定訂單狀態是否可變更
* 決定退款是否合法
* 決定是否扣庫存
* 決定使用者是否有權限

---

## Entity 規則

Entity 是資料庫模型。

Entity 應：

* 對應 MSSQL 資料表
* 對齊欄位型別
* 對齊 Nullable
* 對齊欄位長度
* 對齊關聯

Entity 禁止：

* 直接作為 API Response
* 直接傳到 View
* 包含前端顯示邏輯
* 暴露 PasswordHash、SecurityStamp、內部欄位

---

## DTO / ViewModel 規則

DTO 是 API 輸入與輸出模型。

Request DTO：

* 只接收前端必要欄位
* 必須做驗證
* 不可包含由後端決定的資料，例如價格、權限、角色

Response DTO：

* 只回傳前端需要的欄位
* 不回傳敏感欄位
* 不回傳完整 Entity
* 不回傳內部稽核欄位

DTO 建議依邊界與模組分組：

```text
DTOs/
├─ Storefront/
│  ├─ Auth/
│  ├─ Products/
│  ├─ Cart/
│  ├─ Orders/
│  └─ Payments/
│
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
│  └─ AuditLogs/
│
└─ Common/
   ├─ ApiResponse.cs
   ├─ PagedRequest.cs
   ├─ PagedResult.cs
   └─ ErrorResponse.cs
```

---

## API 邊界

前台 API 與後台 API 必須分開。

### 前台 API

Route 建議：

```text
/api/storefront/*
```

範例：

```text
GET    /api/storefront/products
GET    /api/storefront/products/{id}
GET    /api/storefront/cart
POST   /api/storefront/cart/items
POST   /api/storefront/orders
GET    /api/storefront/me/orders
GET    /api/storefront/me/orders/{id}
POST   /api/storefront/orders/{id}/payments
```

前台 API 核心原則：

* 顧客只能查自己的訂單
* 顧客只能修改自己的購物車
* 商品價格與折扣由後端重新計算
* 不相信前端傳來的權限與價格

---

### 後台 API

Route 建議：

```text
/api/admin/*
```

範例：

```text
GET    /api/admin/products
POST   /api/admin/products
PUT    /api/admin/products/{id}
POST   /api/admin/products/{id}/publish

GET    /api/admin/inventory/stocks
POST   /api/admin/inventory/adjust

GET    /api/admin/orders
GET    /api/admin/orders/{id}
PATCH  /api/admin/orders/{id}/status

GET    /api/admin/payments
GET    /api/admin/shipments
GET    /api/admin/refunds
GET    /api/admin/audit/admin-actions
```

後台 API 核心原則：

* 必須檢查 Role / Permission
* 重要操作寫入 AuditLogs 或 AdminActionLogs
* 權限不可只靠前端隱藏按鈕
* 刪除或狀態異動要謹慎處理

---

## Authentication

正式登入狀態以 Cookie Authentication 為主。

Cookie 應設定：

* HttpOnly
* Secure
* SameSite
* 合理過期時間

JWT 或短期 Token 僅作為特殊場景補充。

禁止：

* 高權限 JWT 長期存放 localStorage
* Cookie / Token 簽章金鑰放在前端
* 將完整 Token 或 Cookie 寫入 Log
* 把登入狀態只交給前端管理

---

## Authorization

本專案使用：

```text
Users
  ↓
UserRoles
  ↓
Roles
  ↓
RolePermissions
  ↓
Permissions
```

常見權限：

```text
Products.Read
Products.Write
Orders.Read
Orders.Write
Inventory.Adjust
Payments.Read
Shipments.Manage
Refunds.Manage
Coupons.Manage
AuditLogs.Read
Users.Manage
Roles.Manage
```

前台 API：

* 主要檢查是否為本人資料

後台 API：

* 主要檢查角色與權限

---

## 密碼安全

禁止：

* 明文密碼
* MD5
* 單純 SHA256(password)
* Base64(password)

建議：

* ASP.NET Core Identity PasswordHasher
* PBKDF2
* bcrypt
* Argon2

登入成功與失敗都應寫入 UserLoginLogs。

---

## EF Core 規則

查詢原則：

* 優先使用 async / await
* 優先使用 EF Core 非同步方法
* 列表查詢預設考慮 AsNoTracking()
* 避免 N+1 Query
* 避免無限制 Include
* 大量資料必須分頁
* 排序欄位必須白名單
* 搜尋條件必須防止 SQL Injection

Entity 對應原則：

* Entity 對齊 MSSQL Table
* Property 對齊 Column
* HasKey 對齊 Primary Key
* HasOne / HasMany 對齊 Foreign Key
* HasIndex 對齊 Index
* HasIndex().IsUnique() 對齊 Unique Key
* HasCheckConstraint 對齊 Check Constraint

---

## Transaction 規則

只要同一個流程會修改多張資料表，就要考慮 Transaction。

必須考慮 Transaction 的流程：

* 建立訂單 + 建立訂單明細 + 保留庫存
* 取消訂單 + 釋放保留庫存 + 訂單狀態歷史
* 付款成功 + 寫入金流交易 + 更新訂單狀態
* 出貨成功 + 扣庫存 + 寫入庫存異動
* 退款成功 + 寫入退款明細 + 更新付款狀態
* 使用優惠券 + 寫入使用紀錄 + 寫入訂單折扣

原則：

```text
要成功就全部成功
要失敗就全部取消
```

---

## 訂單與庫存決策

本專案核心決策：

* 下單時保留庫存
* 出貨時才扣實際庫存
* 取消訂單時釋放保留庫存
* 付款成功後訂單進入 Paid
* 付款成功不等於自動出貨
* 出貨流程與付款流程不可混成同一步
* 退款支援整筆退款與部分退款
* 優惠券與促銷可以疊加，但必須記錄折扣來源

---

## 資安規則

必須檢查：

* Controller 是否加上 Authorize
* Service 是否再次檢查資料權限
* 前台是否只能操作本人資料
* 後台是否檢查 Role / Permission
* DTO 是否避免資料外洩
* 錯誤訊息是否避免 Stack Trace、SQL、檔案路徑
* CORS 是否限制可信來源
* Cookie 是否設定 HttpOnly / Secure / SameSite
* Connection String 是否避免進 Git
* 金流、物流、SMTP Key 是否放在 Secret Manager 或環境變數
* 重要操作是否寫入 AuditLogs 或 AdminActionLogs

禁止：

* AllowAnyOrigin 用於正式環境
* 正式環境公開詳細錯誤
* 正式環境公開 Swagger 而無保護
* 前端傳來的價格直接採用
* 前端傳來的角色或權限直接採用
* 稽核紀錄被一般使用者查詢或刪除

---

## Program.cs 與 Middleware

Program.cs 應保持乾淨。

建議將註冊拆成 Extension Method：

```text
Extensions/
├─ CorsExtensions.cs
├─ AuthenticationExtensions.cs
├─ AuthorizationExtensions.cs
├─ ApplicationServiceExtensions.cs
└─ InfrastructureServiceExtensions.cs
```

Middleware Pipeline 建議順序：

```text
Exception Handling
  ↓
HTTPS Redirection
  ↓
Routing
  ↓
CORS
  ↓
Authentication
  ↓
Authorization
  ↓
Controller Routing
```

---

## 前端規則

前端優先使用：

* Bootstrap 5
* RWD
* Vue Router
* Pinia
* Axios 或 Fetch
* SweetAlert2

避免：

* 大量自訂 CSS
* 在 localStorage 長期保存高權限 Token
* 在前端保存任何後端機密
* 只靠前端隱藏按鈕做權限控制
* 使用 v-html 顯示未清理資料

Axios / Fetch 應集中設定：

* baseURL
* withCredentials
* 401 導回登入頁
* 403 顯示無權限
* 錯誤訊息統一處理
* Loading 狀態

---

## 開發流程

請依照小步驟開發：

1. 確認需求
2. 判斷模組與分層
3. 判斷是否涉及權限
4. 判斷是否涉及 Transaction
5. 判斷是否涉及資安
6. 說明會修改哪些檔案
7. 寫最小可行實作
8. 測試
9. 再重構

---

## AI 回覆格式

AI 提供程式碼前，必須先輸出：

【目的說明】

【目前判斷】

【建議做法】

【會修改的檔案】

【風險與替代方案】

接著才提供：

【完整程式碼】

【測試方式】

【後續建議】

---

## TDD 使用規則

適合使用 TDD 的情境：

* 訂單建立
* 庫存保留
* 優惠券驗證
* 付款狀態更新
* 退款金額檢查
* 權限檢查
* API Response 格式
* Service 商業規則

流程：

1. 先確認驗收條件
2. 先列測試案例
3. 先寫測試或測試規格
4. 實作最小版本
5. 測試通過
6. 重構

---

## Grill Mode 使用規則

當需求不清楚時，AI 不要直接亂寫。

應該只問 1 到 3 個關鍵問題，並提供建議答案。

範例：

```text
問題 1：這個 API 是前台使用還是後台使用？
建議答案：如果是顧客查商品，放在 /api/storefront/products；如果是管理員維護商品，放在 /api/admin/products。

問題 2：這個流程是否會修改庫存？
建議答案：如果會修改庫存，需要放在 Service 並考慮 Transaction。

問題 3：是否需要寫入稽核紀錄？
建議答案：後台新增、修改、刪除商品通常應寫入 AdminActionLogs。
```

---

## 禁止大改原則

AI 不得任意：

* 新增 NuGet 套件
* 改資料庫結構
* 刪除檔案
* 改命名空間
* 改專案架構
* 把 MVC 改成微服務
* 把 Cookie Auth 改成 JWT First
* 把 Storefront / Admin 混成同一套 Controller
* 把 Entity 直接回傳 API
* 把所有邏輯塞進 Controller

若真的需要，必須先說明：

* 原因
* 影響範圍
* 風險
* 替代方案
* 回復方式

---

## 完成定義

一個功能完成不只是能跑。

必須符合：

* Controller / Service / Repository 分層正確
* DTO 不暴露敏感資料
* 權限檢查正確
* 交易流程正確
* EF Core 查詢合理
* 錯誤處理明確
* HTTP Status Code 合理
* 可測試
* 可維護
* 文件未衝突
* 不違反 ADR
---

# 頁面設計與 Views 補充規範

## 目前頁面開發狀態

本專案目前可先使用 ASP.NET Core MVC Razor Views 製作頁面，並以 DayNight Admin HTML / CSS / JS 版型作為視覺基礎。

長期仍需維持前台與後台邊界：

```text
前台 Storefront：商品展示、購物車、結帳、會員訂單
後台 Admin：Dashboard、Analytics、商品、庫存、訂單、付款、出貨、退款、優惠、稽核
帳號 Auth：登入、註冊、忘記密碼
```

## 頁面設計文件

頁面相關需求以此資料夾為準：

```text
docs/頁面設計/
```

其內容包含：

```text
版型來源與整合原則
前台與後台頁面邊界
Views 資料夾規劃
前台頁面設計清單
後台頁面設計清單
登入註冊忘記密碼頁面設計
版型檔案對應與資源放置
切版轉換成 cshtml 步驟
前台匿名與登入後流程
後台權限與選單顯示規則
頁面 ViewModel 與 API 對應
CSS 與 JavaScript 整合規範
頁面設計驗收清單
```

## 前台頁面規則

* 未登入使用者可以看首頁、商品列表、商品詳細、關於我們、登入、註冊、忘記密碼。
* 未登入使用者不可建立訂單、付款、查看我的訂單或修改會員資料。
* 前台不顯示 Dashboard、Analytics、營收、轉換率、後台管理選單。
* 商品價格、優惠、庫存狀態必須由後端提供，不相信前端傳入值。

## 後台頁面規則

* 後台頁面必須登入。
* Customer 不得進入後台。
* Admin / Staff 依 Role / Permission 顯示選單與功能。
* Dashboard、Analytics 與營運數據只能在後台顯示。
* 庫存調整、退款、權限異動、訂單狀態修改必須寫入稽核紀錄。

## Razor View 規則

* View 使用 ViewModel，不直接使用 Entity。
* Controller 呼叫 Service 取得 ViewModel。
* View 不直接操作 DbContext。
* 共用 Layout 拆成 `_StorefrontLayout.cshtml`、`_AdminLayout.cshtml`、`_AuthLayout.cshtml`。
* DayNight 的 CSS / JS 放到 `wwwroot`，不要每頁複製。
* 所有寫入型表單使用 Anti-forgery Token。
