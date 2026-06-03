# CONTEXT.md

## 1. 文件目的

本文件是 MyMVC 電商系統的「專案上下文」。  
提供給使用者、Codex、ChatGPT、Claude Code、Cursor、Windsurf 或其他 AI coding agent 對齊專案架構、術語、分層責任、資安規則、正式上線規範與開發流程。

AI 在修改本專案前，必須先閱讀：

```text
AGENTS.md
CONTEXT.md
README.md
```

若是開發、重構、部署或資料庫任務，還必須閱讀對應的 `docs/*` 文件。

---

## 2. 專案定位

本專案是一套完整交易型電商平台，目標是：

```text
□ 可正式上線
□ 可長期維護
□ 可逐步擴充
□ 可測試
□ 可回復
□ 資安優先
□ 可監控
□ 可備份還原
```

本專案不是：

```text
□ 教學型專案
□ Demo 專案
□ 單表 CRUD 專案
□ 只做後台管理的系統
□ 只做 MVC View 的傳統網站
□ 初期就拆微服務的系統
```

---

## 3. 核心架構

本專案採用：

```text
ASP.NET Core Web API 模組化單體
+
共用 MSSQL Database
+
Storefront Vue App
+
Admin Vue App
+
HttpOnly Secure Cookie Authentication
+
Role / Permission Authorization
```

整體資料流：

```text
Storefront App
  ↓
前台 API /api/storefront/*
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
後台 API /api/admin/*
  ↑
Admin App
```

正式環境登入狀態使用 Cookie Authentication，並搭配 CSRF 防護。JWT 或短期 Token 只能作為特定受控場景的補充。

---

## 4. 專案文件地圖

### 4.1 根目錄文件

| 文件 | 用途 |
|---|---|
| `README.md` | 給人看的專案控管入口、架構總覽、里程碑與開發順序 |
| `CONTEXT.md` | 給 AI 與開發者看的專案上下文與詳細規則 |
| `AGENTS.md` | 給 AI coding agent 的實作守則與回覆格式 |

---

### 4.2 docs/adr

```text
docs/adr/
```

用途：

```text
□ 記錄架構決策
□ 確保文件與程式碼不互相衝突
□ 若要推翻既有決策，必須先提出 ADR 修改建議
```

目前核心 ADR 範圍：

```text
0001～0011：核心系統架構、資料庫、API、前後台、庫存、付款、退款、促銷
0012～0020：正式部署、CORS、Rate Limit、金流 Callback、Swagger、備份、監控、SEO、圖片上傳安全
```

若文件與 ADR 衝突，以 ADR 為優先。

---

### 4.3 docs/MSSQL

```text
docs/MSSQL/
```

用途：

```text
□ 資料庫模組設計
□ 資料表欄位
□ PK / FK / UK / CK / Index
□ 建表 SQL
□ ERD
□ API 與資料表對應
```

Entity、DbContext、Repository、查詢條件與交易流程，必須以 `docs/MSSQL` 為資料庫真實來源。

---

### 4.4 docs/MVC

```text
docs/MVC/
```

用途：

```text
□ 系統總覽
□ EF Core 對應
□ 後端分層
□ Repository / Service
□ Program.cs / Middleware
□ Cookie Authentication / CSRF / 權限
□ RESTful API
□ OWASP / API 資安
□ 完整開發流程
□ 常用指令與模板
```

目前建議文件：

```text
00_總覽與正式上線架構.md
01_資料庫與EFCore正式對應.md
02_後端分層Repository與Service.md
03_Program與Middleware安全管線.md
04_登入驗證Cookie權限與CSRF安全.md
05_前台與後台Vue正式部署流程.md
06_RESTfulAPI前後台API與交易流程.md
07_OWASP與API資安檢查清單.md
08_完整系統開發部署流程清單.md
09_常用指令與安全檔案模板.md
10_ADR決策對照與正式上線校正.md
```

---

### 4.5 docs/正式上線技術規範

```text
docs/正式上線技術規範/
```

用途：

```text
□ DNS / TLS / HSTS
□ 環境分離
□ 機密管理
□ API 安全
□ CI/CD
□ 監控、日誌、告警
□ 備份還原
□ 效能壓測
□ 金流物流合規
□ 前端正式上線
□ 上線驗收
```

正式部署或 Production 設定修改前，必須閱讀此資料夾。

---

### 4.6 docs/頁面設計

```text
docs/頁面設計/
```

用途：

```text
□ Storefront 頁面規劃
□ Admin 頁面規劃
□ 登入、註冊、忘記密碼
□ 商品、購物車、訂單
□ 後台商品、庫存、付款、物流、退款、優惠、稽核管理
```

前端實作必須依頁面設計文件與 API 邊界進行。

---

## 5. 文件優先權

若文件互相衝突，依下列順序判斷：

```text
1. 使用者本次明確要求
2. docs/adr/*
3. docs/正式上線技術規範/*
4. docs/MSSQL/*
5. docs/MVC/*
6. docs/頁面設計/*
7. AGENTS.md
8. CONTEXT.md
9. README.md
```

若新需求會違反 ADR，不得直接修改程式碼；應先提出 ADR 修正建議。

---

## 6. 系統邊界

### 6.1 Storefront App

前台顧客使用。

主要功能：

```text
□ 首頁
□ 商品列表
□ 商品詳細
□ 商品搜尋
□ 商品篩選
□ 會員登入
□ 會員註冊
□ 忘記密碼
□ 加入購物車
□ 查看購物車
□ 結帳
□ 付款
□ 我的訂單
□ 訂單詳細
```

核心規則：

```text
□ 顧客只能操作自己的資料
□ 前台不能傳入可信價格
□ 前台不能決定訂單折扣結果
□ 前台不能決定使用者角色或權限
□ 前台按鈕與畫面不是安全邊界
```

---

### 6.2 Admin App

後台管理員與員工使用。

主要功能：

```text
□ Dashboard
□ 使用者管理
□ 角色權限管理
□ 商品管理
□ SKU 管理
□ 商品圖片管理
□ 庫存查詢
□ 庫存調整
□ 訂單管理
□ 付款管理
□ 出貨管理
□ 退款管理
□ 優惠券管理
□ 促銷管理
□ 稽核紀錄查詢
```

核心規則：

```text
□ 後台 API 必須檢查 Role 或 Permission
□ 隱藏按鈕只是 UX，不是安全控制
□ 重要操作要寫入 AdminActionLogs 或 AuditLogs
□ 權限異動、退款、庫存調整、訂單狀態修改必須嚴格檢查
```

---

## 7. 後端分層

固定分層：

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

### 7.1 Controller 規則

Controller 負責：

```text
□ 接收 HTTP Request
□ 基本 Model Validation
□ 呼叫 Service
□ 回傳 HTTP Status Code
□ 回傳 DTO / ApiResponse
```

Controller 禁止：

```text
□ 寫複雜商業流程
□ 直接操作 DbContext
□ 直接處理多表交易
□ 直接回傳 Entity
□ 決定庫存是否扣除
□ 決定訂單是否可退款
□ 決定付款成功後的複雜狀態流轉
```

命名建議：

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

### 7.2 Service 規則

Service 是商業邏輯核心。

Service 負責：

```text
□ 商業規則
□ 權限判斷
□ 訂單流程
□ 庫存流程
□ 付款流程
□ 出貨流程
□ 退款流程
□ 優惠券驗證
□ DTO 組裝
□ Transaction 控制
□ 呼叫 Repository
□ 寫入稽核紀錄
```

Service 禁止：

```text
□ 回傳 IActionResult
□ 依賴 Controller
□ 直接處理 HTTP Response
□ 把資料庫查詢細節大量散落在 Service 中
```

---

### 7.3 Repository 規則

Repository 負責：

```text
□ 查詢資料
□ 新增資料
□ 更新資料
□ 刪除資料
□ 組合必要 EF Core 查詢
□ 回傳 Entity 或查詢投影結果
```

Repository 禁止：

```text
□ 決定商業規則
□ 決定訂單狀態是否可變更
□ 決定退款是否合法
□ 決定是否扣庫存
□ 決定使用者是否有權限
```

---

### 7.4 Entity 規則

Entity 是資料庫模型。

Entity 應：

```text
□ 對應 MSSQL 資料表
□ 對齊欄位型別
□ 對齊 Nullable
□ 對齊欄位長度
□ 對齊關聯
□ 對齊 Index / Unique / Check Constraint
```

Entity 禁止：

```text
□ 直接作為 API Response
□ 直接傳到 View
□ 包含前端顯示邏輯
□ 暴露 PasswordHash、SecurityStamp、內部欄位
```

---

### 7.5 DTO / ViewModel 規則

Request DTO：

```text
□ 只接收前端必要欄位
□ 必須做驗證
□ 不可包含價格、權限、角色等後端決定資料
```

Response DTO：

```text
□ 只回傳前端需要欄位
□ 不回傳敏感欄位
□ 不回傳完整 Entity
□ 不回傳內部稽核欄位
```

建議結構：

```text
DTOs/
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
│  └─ AuditLogs/
└─ Common/
   ├─ ApiResponse.cs
   ├─ PagedRequest.cs
   ├─ PagedResult.cs
   └─ ErrorResponse.cs
```

---

## 8. API 邊界

前台 API 與後台 API 必須分開。

### 8.1 前台 API

Route：

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
POST   /api/storefront/coupons/validate
```

原則：

```text
□ 顧客只能查自己的訂單
□ 顧客只能修改自己的購物車
□ 商品價格與折扣由後端重新計算
□ 不相信前端傳來的權限與價格
```

---

### 8.2 後台 API

Route：

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

原則：

```text
□ 必須檢查 Role / Permission
□ 重要操作寫入 AuditLogs 或 AdminActionLogs
□ 權限不可只靠前端隱藏按鈕
□ 刪除或狀態異動要謹慎處理
```

---

## 9. Authentication / Authorization

### 9.1 Authentication

正式登入狀態以 Cookie Authentication 為主。

Cookie 必須規劃：

```text
□ HttpOnly
□ Secure
□ SameSite
□ 合理過期時間
□ 登出清除 Cookie
□ 權限變更後處理既有登入狀態
```

正式環境必須搭配 CSRF 防護。

JWT 或短期 Token 僅作為特殊場景補充，不得作為高權限長效登入狀態長期放在 localStorage。

---

### 9.2 Authorization

本專案權限模型：

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

```text
主要檢查是否為本人資料。
```

後台 API：

```text
主要檢查角色與權限。
```

---

## 10. 密碼安全

禁止：

```text
□ 明文密碼
□ MD5
□ 單純 SHA256(password)
□ Base64(password)
```

建議：

```text
□ ASP.NET Core Identity PasswordHasher
□ PBKDF2
□ bcrypt
□ Argon2
```

登入成功與失敗都應寫入 `UserLoginLogs`。

---

## 11. EF Core 規則

查詢原則：

```text
□ 優先使用 async / await
□ 優先使用 EF Core 非同步方法
□ 列表查詢預設考慮 AsNoTracking()
□ 避免 N+1 Query
□ 避免無限制 Include
□ 大量資料必須分頁
□ 排序欄位必須白名單
□ 搜尋條件必須防止 SQL Injection
```

Entity 對應原則：

```text
□ Entity 對齊 MSSQL Table
□ Property 對齊 Column
□ HasKey 對齊 Primary Key
□ HasOne / HasMany 對齊 Foreign Key
□ HasIndex 對齊 Index
□ HasIndex().IsUnique() 對齊 Unique Key
□ HasCheckConstraint 對齊 Check Constraint
```

---

## 12. Transaction 規則

只要同一個流程會修改多張資料表，就要考慮 Transaction。

必須考慮 Transaction 的流程：

```text
□ 建立訂單 + 建立訂單明細 + 保留庫存
□ 取消訂單 + 釋放保留庫存 + 訂單狀態歷史
□ 付款成功 + 寫入金流交易 + 更新訂單狀態
□ 出貨成功 + 扣庫存 + 寫入庫存異動
□ 退款成功 + 寫入退款明細 + 更新付款狀態
□ 使用優惠券 + 寫入使用紀錄 + 寫入訂單折扣
```

原則：

```text
要成功就全部成功
要失敗就全部取消
```

---

## 13. 訂單、庫存、付款、退款核心決策

```text
□ 下單時保留庫存
□ 出貨時才扣實際庫存
□ 取消訂單時釋放保留庫存
□ 付款成功後訂單進入 Paid
□ 付款成功不等於自動出貨
□ 出貨流程與付款流程不可混成同一步
□ 退款支援整筆退款與部分退款
□ 優惠券與促銷可以疊加，但必須記錄折扣來源
□ 金流 Callback 必須驗簽並具備冪等性
```

---

## 14. 正式上線資安規則

必須檢查：

```text
□ Controller 是否加上 Authorize
□ Service 是否再次檢查資料權限
□ 前台是否只能操作本人資料
□ 後台是否檢查 Role / Permission
□ DTO 是否避免資料外洩
□ 錯誤訊息是否避免 Stack Trace、SQL、檔案路徑
□ CORS 是否限制可信來源
□ Cookie 是否設定 HttpOnly / Secure / SameSite
□ CSRF 是否保護寫入型 API
□ Rate Limit 是否保護登入、註冊、優惠券、搜尋、Callback
□ Connection String 是否避免進 Git
□ 金流、物流、SMTP Key 是否放在 Secret Manager 或環境變數
□ 重要操作是否寫入 AuditLogs 或 AdminActionLogs
□ Swagger 正式環境是否關閉或加權限
□ 是否有 Health Check
□ 是否有 Log、Metric、Trace、Alert
□ 是否有資料庫備份與還原演練
```

禁止：

```text
□ AllowAnyOrigin 用於正式環境
□ 正式環境公開詳細錯誤
□ 正式環境公開 Swagger 而無保護
□ 前端傳來的價格直接採用
□ 前端傳來的角色或權限直接採用
□ 稽核紀錄被一般使用者查詢或刪除
□ Log 記錄密碼、完整 Token、完整 Cookie、信用卡敏感資料
```

---

## 15. Program.cs 與 Middleware

Program.cs 應保持乾淨。

建議拆成 Extension Method：

```text
Extensions/
├─ CorsExtensions.cs
├─ AuthenticationExtensions.cs
├─ AuthorizationExtensions.cs
├─ ApplicationServiceExtensions.cs
├─ InfrastructureServiceExtensions.cs
├─ SwaggerExtensions.cs
├─ RateLimitExtensions.cs
└─ HealthCheckExtensions.cs
```

Middleware Pipeline 建議順序：

```text
Exception Handling / ProblemDetails
  ↓
Security Headers
  ↓
HTTPS Redirection
  ↓
HSTS
  ↓
Routing
  ↓
CORS
  ↓
Rate Limiting
  ↓
Authentication
  ↓
Authorization
  ↓
CSRF 驗證
  ↓
Controller Routing
```

---

## 16. 前端規則

前端優先使用：

```text
□ Bootstrap 5
□ RWD
□ Vue Router
□ Pinia
□ Axios 或 Fetch
□ SweetAlert2
```

避免：

```text
□ 大量自訂 CSS
□ 在 localStorage 長期保存高權限 Token
□ 在前端保存任何後端機密
□ 只靠前端隱藏按鈕做權限控制
□ 使用 v-html 顯示未清理資料
```

Axios / Fetch 應集中設定：

```text
□ baseURL
□ withCredentials
□ CSRF Header
□ 401 導回登入頁
□ 403 顯示無權限
□ 429 顯示操作太頻繁
□ 錯誤訊息統一處理
□ Loading 狀態
```

---

## 17. 開發流程

每次開發請依照小步驟：

```text
1. 確認需求
2. 查閱 ADR
3. 查閱對應 MSSQL 文件
4. 查閱 MVC 或前端文件
5. 查閱正式上線規範是否涉及資安
6. 判斷模組與分層
7. 判斷是否涉及權限
8. 判斷是否涉及 Transaction
9. 說明會修改哪些檔案
10. 寫最小可行實作
11. 測試
12. 再重構
13. 更新必要文件
```

---

## 18. AI 回覆格式

AI 提供程式碼前，必須先輸出：

```text
【目的說明】

【目前判斷】

【參考文件】

【建議做法】

【會修改的檔案】

【是否涉及權限】

【是否涉及 Transaction】

【是否涉及資安】

【風險與替代方案】
```

接著才提供：

```text
【完整程式碼】

【測試方式】

【完成檢查】

【後續建議】
```

---

## 19. TDD 使用規則

適合使用 TDD 或至少先列測試案例的情境：

```text
□ 訂單建立
□ 庫存保留
□ 優惠券驗證
□ 付款狀態更新
□ 金流 Callback 冪等
□ 退款金額檢查
□ 權限檢查
□ API Response 格式
□ Service 商業規則
```

流程：

```text
1. 先確認驗收條件
2. 先列測試案例
3. 先寫測試或測試規格
4. 實作最小版本
5. 測試通過
6. 重構
```

---

## 20. Grill Mode 使用規則

需求不清楚時，AI 不要直接亂寫。

應只問 1 到 3 個關鍵問題，並提供建議答案。

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

## 21. 禁止大改原則

AI 不得任意：

```text
□ 新增 NuGet 套件
□ 改資料庫結構
□ 刪除檔案
□ 改命名空間
□ 改專案架構
□ 把 MVC 改成微服務
□ 把 Cookie Auth 改成 JWT First
□ 把 Storefront / Admin 混成同一套 Controller
□ 把 Entity 直接回傳 API
□ 把所有邏輯塞進 Controller
```

若真的需要，必須先說明：

```text
□ 原因
□ 影響範圍
□ 風險
□ 替代方案
□ 回復方式
```

---

## 22. 專案控管里程碑

| 里程碑 | 目標 | 主要文件 | 完成標準 |
|---|---|---|---|
| M0 | 文件與架構決策完成 | `README.md`, `CONTEXT.md`, `docs/adr/*` | 文件無衝突 |
| M1 | MSSQL 建立 | `docs/MSSQL/*` | 建表、PK/FK/Index、測試資料完成 |
| M2 | 後端骨架 | `docs/MVC/00~03` | Solution、四層專案、Program.cs 完成 |
| M3 | Entity / DbContext | `docs/MVC/01` | Entity 對齊資料表並可查詢 |
| M4 | Auth / Permission | `docs/MVC/04` | Cookie、CSRF、Role、Permission、UserLoginLogs 完成 |
| M5 | Catalog / Inventory | `docs/MVC/06`, `docs/MSSQL/*` | 商品、SKU、庫存 API 完成 |
| M6 | Cart / Order | `docs/MVC/06`, ADR-0003 | 建立訂單與保留庫存完成 |
| M7 | Payment / Shipment / Refund | ADR-0004, ADR-0005, ADR-0015 | Callback、出貨、退款流程完成 |
| M8 | Promotion / Audit | ADR-0006 | 折扣紀錄與稽核完成 |
| M9 | Storefront Vue | `docs/MVC/05`, `docs/頁面設計/*` | 前台主要頁面可用 |
| M10 | Admin Vue | `docs/MVC/05`, `docs/頁面設計/*` | 後台主要頁面可用 |
| M11 | 正式部署準備 | `docs/正式上線技術規範/*` | CORS、Rate Limit、Health Check、Log、備份完成 |
| M12 | Production 啟用 | `docs/正式上線技術規範/10_*` | 上線驗收完成並可回滾 |

---

## 23. 完成定義

一個功能完成不只是能跑，必須符合：

```text
□ Controller / Service / Repository 分層正確
□ DTO 不暴露敏感資料
□ 權限檢查正確
□ 交易流程正確
□ EF Core 查詢合理
□ 錯誤處理明確
□ HTTP Status Code 合理
□ 可測試
□ 可維護
□ 文件未衝突
□ 不違反 ADR
□ 若為正式部署功能，符合正式上線規範
```
