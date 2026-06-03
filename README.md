# MyMVC 全端電商系統

## 1. 專案簡介

MyMVC 是一套以正式部署與長期維護為目標的全端電商系統。

系統包含：

```text
Storefront App：前台顧客購物網站
Admin App：後台管理系統
ASP.NET Core Web API：前台與後台共用後端
EF Core：資料存取
MSSQL Database：共用資料庫
Cookie Authentication：正式登入狀態
Role / Permission：後台權限控管
Audit Logs：重要操作紀錄
正式上線技術規範：部署、資安、監控、備份與驗收
```

本專案不是教學型單表 CRUD，而是以正式商業電商系統為目標進行設計。

---

## 2. 專案控管入口

本專案以 Markdown 文件控管架構、資料庫、API、前端、正式上線與 AI 開發規則。

最重要的三份入口文件：

| 文件 | 用途 |
|---|---|
| `README.md` | 專案總覽、文件地圖、里程碑、開發順序 |
| `CONTEXT.md` | 專案上下文、分層規則、API 邊界、資安規則 |
| `AGENTS.md` | AI coding agent 實作守則、回覆格式、禁止事項 |

開發前請先閱讀：

```text
AGENTS.md
CONTEXT.md
README.md
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
Service
  ↓
Repository
  ↓
EF Core
  ↓
MSSQL Database

Admin App
  ↓
後台 API /api/admin/*
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

## 4. 技術棧

### Backend

```text
C#
ASP.NET Core Web API
ASP.NET Core MVC
EF Core
MSSQL
Cookie Authentication
Role / Permission Authorization
```

### Frontend

```text
Vue
Vue Router
Pinia
Axios 或 Fetch
Bootstrap 5
SweetAlert2
RWD
```

### Database

```text
Microsoft SQL Server
MSSQL Create Table Script
ERD 文件
模組化資料表設計
```

### Deployment / Operation

```text
DNS / TLS / HSTS
CORS 白名單
CSRF 防護
Rate Limiting
Security Headers
Health Check
Log / Metric / Trace / Alert
CI/CD
資料庫備份還原
正式上線驗收
```

---

## 5. 文件導覽

### 5.1 架構決策

```text
docs/adr/
```

用途：

```text
□ 記錄已確認的架構決策
□ 避免開發中出現互相矛盾的做法
□ 若新設計與 ADR 衝突，應先新增或修正 ADR
```

重點：

```text
0001～0011：核心架構、資料庫、API、前後台、交易流程
0012～0020：正式部署、資安、監控、金流 Callback、圖片上傳安全
```

---

### 5.2 資料庫設計

```text
docs/MSSQL/
```

用途：

```text
□ 資料庫模組設計
□ 資料表欄位
□ PK / FK / Index / Check Constraint
□ 建表 SQL
□ ERD 關聯
□ API 與資料表對應
```

後端開發時，Entity 與 DbContext 應以此資料夾內容為準。

---

### 5.3 MVC / Web API 架構

```text
docs/MVC/
```

用途：

```text
□ ASP.NET Core Web API 架構規劃
□ EF Core 對應規則
□ Repository / Service 分層
□ Program.cs 與 Middleware Pipeline
□ Cookie Authentication / CSRF
□ RESTful API
□ OWASP / API 資安
□ 完整開發流程
□ 常用指令與安全模板
```

---

### 5.4 正式上線技術規範

```text
docs/正式上線技術規範/
```

用途：

```text
□ 正式環境設定
□ HTTPS / DNS / TLS / HSTS
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

---

### 5.5 頁面設計

```text
docs/頁面設計/
```

用途：

```text
□ Storefront 頁面設計
□ Admin 頁面設計
□ 登入、註冊、忘記密碼
□ 商品、購物車、訂單
□ 後台商品、庫存、付款、物流、退款、優惠、稽核管理
```

---

## 6. 建議專案結構

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
├─ AGENTS.md
├─ CONTEXT.md
└─ README.md
```

---

## 7. 核心模組

| 模組 | 負責內容 | 主要資料表 |
|---|---|---|
| 帳戶與權限 | 登入、角色、權限、登入紀錄 | Users, Roles, UserRoles, Permissions, RolePermissions, UserLoginLogs |
| 商品目錄 | 商品分類、商品、SKU、圖片、規格 | ProductCategories, Products, ProductSkus, ProductImages, ProductAttributes, ProductAttributeValues |
| 庫存 | 倉庫、庫存量、庫存異動、保留庫存 | Warehouses, InventoryStocks, InventoryTransactions, InventoryReservations |
| 購物車與訂單 | 購物車、訂單、明細、狀態歷史 | ShoppingCarts, ShoppingCartItems, Orders, OrderItems, OrderStatusHistories |
| 付款物流退款 | 付款、金流交易、出貨、退款 | Payments, PaymentTransactions, Shipments, ShipmentItems, Refunds, RefundItems |
| 優惠促銷 | 優惠券、促銷、折扣紀錄 | Coupons, CouponUsages, Promotions, PromotionProducts, OrderDiscounts |
| 稽核與系統紀錄 | 後台操作、資料異動、錯誤紀錄 | AuditLogs, AdminActionLogs, SystemErrorLogs |

---

## 8. 開發原則

請遵守：

```text
□ 小步修改
□ 可測試
□ 可回復
□ 安全優先
□ 不任意大規模重構
□ 不直接修改資料庫設計
□ 不把商業邏輯寫在 Controller
□ 不把 Entity 直接回傳給 API
□ 不把 Repository 寫成商業流程中心
□ 不把權限判斷只放在前端
□ 不把正式登入狀態設計成 localStorage 高權限 JWT
□ 正式部署必須符合 docs/正式上線技術規範
```

---

## 9. 後端分層

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

| 層級 | 負責 | 不負責 |
|---|---|---|
| Controller | Request、Validation、呼叫 Service、HTTP Status Code | 商業流程、多表交易、直接操作 DbContext |
| Service | 商業邏輯、權限、交易流程、DTO 組裝、Transaction | HTTP 細節 |
| Repository | EF Core 查詢、新增、修改、刪除 | 決定商業規則 |
| Entity | 對應 MSSQL 資料表 | 直接回傳 API |
| DTO | API 輸入輸出 | 保存資料庫完整結構 |

---

## 10. API 邊界

前台與後台 API 必須分開。

```text
前台 API：/api/storefront/*
後台 API：/api/admin/*
```

前台 API：

```text
□ 顧客只能操作自己的資料
□ 商品價格與折扣由後端重新計算
□ 不相信前端傳來的角色、權限、價格
```

後台 API：

```text
□ 必須檢查 Role / Permission
□ 重要操作寫入 AuditLogs 或 AdminActionLogs
□ 權限不可只靠前端隱藏按鈕
□ 退款、庫存調整、權限異動需要嚴格檢查
```

---

## 11. 正式部署底線

Production 不得缺少下列基本項目：

```text
□ HTTPS
□ HSTS 規劃
□ CORS 白名單
□ Cookie HttpOnly / Secure / SameSite
□ CSRF 防護
□ Rate Limiting
□ Security Headers
□ ProblemDetails / 全域錯誤處理
□ Swagger 關閉或加保護
□ Production Secret 不進 Git
□ Health Check
□ Log / Metric / Trace / Alert
□ 資料庫備份與還原演練
□ 金流 Callback 驗簽與冪等
□ 部署可回滾
```

---

## 12. 專案控管里程碑

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

## 13. 每次開發前的工作單

每次請 AI 或自己開發功能前，建議先建立工作單。

```text
【功能名稱】

【功能目標】

【使用者角色】
□ Storefront 顧客
□ Admin 管理員
□ Staff 員工
□ 系統排程 / 第三方 Callback

【參考文件】
□ docs/adr/
□ docs/MSSQL/
□ docs/MVC/
□ docs/正式上線技術規範/
□ docs/頁面設計/

【分層影響】
□ Controller
□ Service
□ Repository
□ Entity
□ DTO
□ Vue View
□ Pinia Store
□ API Module
□ Database
□ Deployment

【資安判斷】
□ 是否需要登入
□ 是否需要 Role / Permission
□ 是否需要 CSRF
□ 是否需要 Rate Limit
□ 是否需要 AuditLogs / AdminActionLogs

【交易判斷】
□ 是否修改多張表
□ 是否需要 Transaction
□ 是否需要冪等
□ 是否需要回滾測試

【完成標準】
□ API 測試通過
□ 前端串接通過
□ 401 / 403 測試通過
□ 錯誤輸入測試通過
□ 稽核紀錄確認
□ 文件無衝突
```

---

## 14. 建議開發順序

```text
1. 確認 ADR 與文件無衝突
2. 建立 MSSQL Database
3. 建立 ASP.NET Core Web API 四層專案
4. 建立 Entity / DbContext
5. 建立 Repository / Service 分層
6. 建立共用 ApiResponse / PagedResult / ErrorResponse
7. 建立 Cookie Authentication + CSRF
8. 建立 Role / Permission
9. 建立商品目錄 API
10. 建立庫存 API
11. 建立購物車與訂單 API
12. 建立付款、物流、退款 API
13. 建立優惠與促銷 API
14. 建立稽核紀錄 API
15. 建立 Storefront App
16. 建立 Admin App
17. 加入 OWASP / API Security 檢查
18. 加入 Health Check / Log / Alert
19. Staging 測試
20. Production 部署
```

---

## 15. AI 開發注意事項

若使用 Codex、ChatGPT、Claude Code、Cursor 或其他 AI 工具，請要求 AI 先閱讀：

```text
AGENTS.md
CONTEXT.md
README.md
docs/adr/*
docs/MSSQL/*
docs/MVC/*
docs/正式上線技術規範/*
docs/頁面設計/*
```

AI 產生程式碼前，必須先說明：

```text
□ 目的
□ 參考文件
□ 會修改哪些檔案
□ 屬於哪個分層
□ 是否涉及權限
□ 是否涉及 Transaction
□ 是否涉及資安
□ 測試方式
□ 風險與替代方案
```

---

## 16. 不應該做的事

請避免：

```text
□ Controller 直接操作 DbContext
□ Controller 寫大量商業邏輯
□ Entity 直接回傳前端
□ 密碼明文儲存
□ PasswordHash 傳給前端
□ localStorage 長期保存高權限 Token
□ AllowAnyOrigin 用於正式環境
□ Swagger 直接暴露於正式環境
□ 忽略 Transaction
□ 忽略 Audit Log
□ 忽略前台與後台 API 分界
□ 把付款成功與出貨流程混成同一步
□ 未經 ADR 討論就改核心架構
```

---

## 17. 完成定義

一個功能完成，不只是畫面能動或 API 回 200。

必須符合：

```text
□ Controller / Service / Repository 分層正確
□ DTO 不暴露敏感資料
□ 權限檢查正確
□ 交易流程正確
□ EF Core 查詢合理
□ 錯誤處理明確
□ HTTP Status Code 合理
□ 可測試
□ 可回復
□ 文件未衝突
□ 不違反 ADR
□ 若涉及 Production，符合正式上線技術規範
```

---

## 18. 下一步

建議先完成：

```text
1. 確認 docs/adr、docs/MSSQL、docs/MVC、docs/正式上線技術規範、docs/頁面設計 都已放入專案
2. 建立後端 Solution 與四層專案
3. 建立 Entity 與 DbContext 對應 MSSQL
4. 建立 AuthService、Cookie Authentication、CSRF、Role / Permission
5. 建立前台 / 後台 API Controller 邊界
6. 從商品目錄 API 開始實作第一個完整模組
```
