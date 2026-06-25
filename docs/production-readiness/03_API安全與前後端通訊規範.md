# 03｜API 安全與前後端通訊規範

> 版本：2026-06-02  
> 文件目的：確保前台 API 與後台 API 在正式環境中具備驗證、授權、CSRF、CORS、Rate Limiting 與安全錯誤處理。

---

## 1. 本章目的

電商網站的 API 會處理登入、商品、訂單、付款、優惠券與後台操作，因此 API 安全不能只靠前端判斷。所有重要規則都必須由後端檢查。

正式架構中，API 分成：

```text
前台 API：/api/storefront/*
後台 API：/api/admin/*
```

---

## 2. API 基本安全

```text
□ 所有正式 API 使用 HTTPS
□ 需要登入的 API 加上 [Authorize]
□ 管理 API 加上 Role 或 Permission Policy
□ 前台 API 檢查資料擁有者
□ 後台 API 檢查管理權限
□ 不直接回傳 Entity
□ 使用 DTO 控制輸入與輸出
□ 輸入資料在後端驗證
□ 錯誤訊息不暴露 Stack Trace
□ 大量資料查詢使用分頁
□ 所有寫入型流程檢查交易狀態與資料一致性
```

---

## 3. Cookie、SameSite 與 CSRF

正式環境以 HttpOnly Secure Cookie 作為瀏覽器登入狀態時，需要同時處理 CSRF。

建議：

```text
□ Cookie HttpOnly = true
□ Cookie Secure = true
□ Cookie SameSite 依部署網域評估 Lax / Strict / None
□ 使用 Cookie 驗證的 POST / PUT / PATCH / DELETE 需有 CSRF 防護
□ 前端 Axios / Fetch 以 withCredentials 呼叫 API
□ 不在 localStorage 長期保存高權限 Token
□ 登出時清除 Cookie 並使伺服器端登入狀態失效
```

常見策略：

```text
1. 後端發送不可 HttpOnly 的 CSRF Token Cookie 或提供取得 Token API
2. 前端把 CSRF Token 放入自訂 Header，例如 X-CSRF-TOKEN
3. 後端驗證 Cookie 與 Header Token 是否匹配
4. 僅對安全方法 GET / HEAD / OPTIONS 放寬
```

---

## 4. CORS 規範

正式環境只允許正式前端網域。

```text
允許：
https://www.example.com
https://admin.example.com

禁止：
AllowAnyOrigin
AllowAnyOrigin + AllowCredentials
```

正式上線使用 HttpOnly Secure Cookie 時，還要確認：

```text
□ Access-Control-Allow-Origin 只能是明確白名單
□ Access-Control-Allow-Credentials 僅在必要時開啟
□ 前台與後台來源分開設定
□ Staging 與 Production CORS 白名單不可混用
□ 不用 CORS 當作授權機制，真正權限仍在後端驗證
```

---

## 5. Rate Limiting

建議限制高風險 API：

```text
□ 登入 API
□ 註冊 API
□ 忘記密碼 API
□ 重設密碼 API
□ 優惠券驗證 API
□ 搜尋 API
□ 金流 Callback API
□ 後台報表查詢 API
□ 圖片上傳 API
```

建議策略：

| API 類型 | 建議限制 |
|---|---|
| 登入 | 依 IP + 帳號雙重限制 |
| 忘記密碼 | 依 Email / IP 限制 |
| 優惠券驗證 | 依 UserId / IP 限制 |
| 搜尋 | 依 IP 或 UserId 限制 |
| 後台高風險操作 | 依 UserId 限制並寫稽核 |

Rate Limit 的目的是避免暴力破解、惡意刷 API 或拖垮系統。正式部署前應在 Staging 壓測，避免限制過嚴導致正常使用者被擋。

---

## 6. Security Headers

正式環境建議設定：

```text
Strict-Transport-Security
X-Content-Type-Options: nosniff
X-Frame-Options 或 Content-Security-Policy frame-ancestors
Referrer-Policy
Content-Security-Policy
Permissions-Policy
```

前台與後台可使用不同 CSP：前台可能需要圖片、字型、金流頁面；後台則應更嚴格限制外部資源。

---

## 7. API 版本規劃

正式上線後，API 不應隨意破壞前端。

建議：

```text
/api/v1/storefront/products
/api/v1/admin/orders
```

或至少在文件中標記 API 版本與變更紀錄。

---

## 8. Webhook 安全

金流、物流常會透過 Webhook 通知系統。

檢查：

```text
□ 驗證簽章
□ 驗證時間戳避免重放攻擊
□ 記錄 Webhook 原始事件 ID
□ 同一事件重送時不可重複扣款或重複出貨
□ Webhook 錯誤要可重試
□ Callback 不信任前端資料，只信任第三方驗簽後的資料
□ Webhook Response 保持簡潔，不暴露內部錯誤
```

---

## 9. 錯誤處理與 ProblemDetails

正式 API 應統一錯誤格式：

```text
400：輸入錯誤
401：未登入
403：無權限
404：找不到資料
409：狀態衝突或重複操作
422：商業規則不通過
429：請求過多
500：系統錯誤
```

正式環境不可回傳：

```text
SQL 內容
Stack Trace
檔案實體路徑
Connection String
第三方 API Key
完整 Cookie / Token
```

---

## 10. 本章總結

前端只負責操作體驗，不能被當作安全邊界。API 必須自行完成驗證、授權、輸入檢查、CSRF、CORS、Rate Limit、錯誤處理與重要流程保護，且前台 API 與後台 API 必須維持清楚邊界。

---

## 參考基準

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- OWASP API Security Top 10 2023: https://owasp.org/API-Security/editions/2023/en/0x11-t10/
- OWASP CSRF Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Microsoft ASP.NET Core CORS: https://learn.microsoft.com/aspnet/core/security/cors
- Microsoft ASP.NET Core Anti-forgery: https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- Microsoft ASP.NET Core Rate Limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Microsoft ASP.NET Core Health Checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
