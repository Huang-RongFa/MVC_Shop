# 03｜Program.cs、Middleware 與安全管線

## 1. 本章目的

`Program.cs` 是 ASP.NET Core 啟動設定與 Middleware Pipeline 的入口。正式部署時，它不只負責讓 API 跑起來，還負責建立錯誤處理、HTTPS、CORS、Rate Limiting、Cookie 驗證、授權、健康檢查與安全標頭等基礎防線。

初學可以這樣理解：

```text
builder.Services：註冊系統會用到的服務
app.Use...：設定每個 HTTP Request 進入系統後會經過哪些處理
app.MapControllers：對外開放 API Controller
```

---

## 2. Program.cs 應負責的設定

| 類型 | 內容 | 正式上線注意 |
|---|---|---|
| Controller | 啟用 Web API Controller | 啟用統一 Model Validation |
| Swagger / OpenAPI | 開發與測試 API | Production 不公開，或加上授權保護 |
| DbContext | 連線 MSSQL | 連線字串不可寫死 |
| CORS | 允許 Vue 前端呼叫 API | 僅允許正式網域，Cookie 情境不可 AllowAnyOrigin |
| Authentication | Cookie Authentication | HttpOnly、Secure、SameSite、過期時間 |
| Authorization | Role / Permission | Policy 化管理 |
| Antiforgery | CSRF 防護 | Cookie 驗證的 SPA 必須規劃 |
| Rate Limiting | 防止暴力登入與濫用 API | 登入、搜尋、金流回呼要特別限制 |
| Exception Handling | 統一錯誤處理 | 不暴露 Stack Trace |
| Health Checks | 監控 API 與 DB 狀態 | 不暴露機密細節 |
| Security Headers | 安全標頭 | HSTS、X-Content-Type-Options、CSP 視情境加入 |
| Application Services | Auth、User、Product、Order 等 | 使用 Extension Method 分組 |
| Infrastructure Services | Repository、DbContext、交易控制 | 使用 DI 管理生命週期 |

---

## 3. 建議拆成 Extension Methods

避免 `Program.cs` 變得太長。

```text
MySystem.Api/
└─ Extensions/
   ├─ CorsExtensions.cs
   ├─ AuthenticationExtensions.cs
   ├─ AuthorizationExtensions.cs
   ├─ AntiforgeryExtensions.cs
   ├─ RateLimitingExtensions.cs
   ├─ SwaggerExtensions.cs
   ├─ HealthCheckExtensions.cs
   ├─ ApplicationServiceExtensions.cs
   └─ InfrastructureServiceExtensions.cs
```

`Program.cs` 保持乾淨，只呈現主流程。

---

## 4. Service 註冊規劃

Application 層：

```text
AuthService
UserService
RoleService
PermissionService
ProductService
ProductSkuService
InventoryService
OrderService
PaymentService
ShipmentService
RefundService
CouponService
PromotionService
AuditService
CurrentUserService
PermissionChecker
```

Infrastructure 層：

```text
AppDbContext
UserRepository
RoleRepository
ProductRepository
InventoryRepository
OrderRepository
PaymentRepository
ShipmentRepository
RefundRepository
CouponRepository
AuditRepository
UnitOfWork 或 TransactionService
ExternalPaymentClient
ExternalShipmentClient
```

---

## 5. Middleware Pipeline 建議順序

正式 API 建議順序：

```text
Exception Handling / ProblemDetails
    ↓
Forwarded Headers（有反向代理時）
    ↓
HTTPS Redirection / HSTS
    ↓
Security Headers
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
Antiforgery 驗證（Cookie + SPA 寫入操作）
    ↓
Controller Routing
    ↓
Health Checks
```

常見問題：

| 問題 | 可能原因 |
|---|---|
| Vue 呼叫 API 被瀏覽器擋掉 | CORS 未設定、來源不一致、credentials 未開 |
| Cookie 正確但 API 仍 401 | SameSite、Domain、Secure、CORS 或驗證設定錯誤 |
| `[Authorize]` 沒有效果 | Authentication / Authorization 未註冊或順序錯 |
| 登入 API 被暴力嘗試 | 未加 Rate Limiting / Lockout |
| 正式環境露出詳細錯誤 | Exception Handling 未依環境區分 |
| 健康檢查外洩內部資訊 | Health Check 回應太詳細或未限制 |

---

## 6. CORS 規劃

本機開發常見來源：

```text
Storefront：http://localhost:5173
Admin：http://localhost:5174
API：https://localhost:5001
```

正式環境範例：

```text
https://www.example.com
https://admin.example.com
https://api.example.com
```

Cookie 驗證與跨來源呼叫時：

```text
前端：withCredentials = true 或 credentials: 'include'
後端：WithOrigins(...).AllowCredentials()
```

正式環境禁止：

```text
AllowAnyOrigin + AllowCredentials
```

原因：這會讓任意網站嘗試攜帶使用者 Cookie 呼叫你的 API，增加 CSRF 與資料外洩風險。

---

## 7. Cookie 與 CSRF

只要使用瀏覽器自動攜帶 Cookie，就必須考慮 CSRF。

建議：

```text
□ 登入 Cookie 使用 HttpOnly + Secure + SameSite
□ 寫入型 API 使用 CSRF Token
□ 前端啟動時取得 XSRF Token
□ Axios / Fetch 將 XSRF Token 放入 Header
□ 後端驗證 Header Token 與 Cookie Token
□ GET 查詢不改資料，POST/PUT/PATCH/DELETE 才做資料異動
```

常見 Header：

```text
X-CSRF-TOKEN
X-XSRF-TOKEN
```

---

## 8. Rate Limiting 規劃

Rate Limiting 不是完整 DDoS 防護，但可以降低 API 被濫用、暴力登入與資源耗盡的風險。

建議限制：

| API | 建議策略 |
|---|---|
| `/api/storefront/auth/login` | 依 IP + Account 限制 |
| `/api/admin/auth/login` | 更嚴格限制，並搭配帳號鎖定 |
| `/api/storefront/products` | 一般速率限制與分頁限制 |
| `/api/admin/*` | 依使用者與角色限制 |
| 金流 Callback | 依來源、驗簽、交易編號冪等處理 |
| 搜尋 / 報表 | 限制頻率、日期範圍與分頁大小 |

---

## 9. Health Checks

正式部署建議加入健康檢查端點。

範例：

```text
/health/live：API 進程是否活著
/health/ready：API 是否可接流量，例如 DB 是否可連
```

注意：

```text
□ 不回傳 Connection String
□ 不回傳資料庫帳密
□ 不回傳詳細例外堆疊
□ 可限制只讓負載平衡器或監控系統存取
```

---

## 10. Swagger / OpenAPI 管理

開發環境可以開 Swagger。

正式環境建議：

```text
□ 關閉 Swagger
或
□ 只允許內部網路
或
□ 加上管理員授權
或
□ 只在 Staging 使用
```

Swagger 不應暴露：

```text
內部管理 API 細節
測試帳密
金流 Callback 密鑰
正式環境錯誤訊息
```

---

## 11. Program.cs 檢查清單

```text
□ AddControllers
□ AddProblemDetails 或全域 Exception Handler
□ AddEndpointsApiExplorer
□ AddSwaggerGen，但 Production 不公開
□ AddDbContext 連線 MSSQL
□ AddCors 並限制前端來源
□ AddAuthentication 設定 Cookie Authentication
□ AddAuthorization 設定 Role / Permission Policy
□ AddAntiforgery 規劃 CSRF
□ AddRateLimiter 規劃登入與高風險 API
□ AddHealthChecks 檢查 API 與 DB
□ 註冊 Application Services
□ 註冊 Infrastructure Repositories
□ UseExceptionHandler
□ UseHttpsRedirection
□ UseHsts（依部署架構評估）
□ UseRouting
□ UseCors
□ UseRateLimiter
□ UseAuthentication
□ UseAuthorization
□ MapControllers
□ MapHealthChecks
```

---

## 12. 本章總結

`Program.cs` 是正式部署的第一道技術防線。它應保持乾淨，但不能簡化到只有 Controller、Swagger 與 DbContext。正式上線前，至少要完成 CORS、Cookie、CSRF、Rate Limiting、Exception Handling、Health Checks、Swagger 管控與安全標頭規劃。
