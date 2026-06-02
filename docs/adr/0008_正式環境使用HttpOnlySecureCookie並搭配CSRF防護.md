# ADR-0008｜正式環境使用 HttpOnly Secure Cookie 並搭配 CSRF 防護

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`03_Program與Middleware安全管線.md`、`04_登入驗證Cookie權限與CSRF安全.md`、`07_OWASP與API資安檢查清單.md`

## 1. 背景與問題

正式環境需要保護會員資料、訂單、付款資訊與後台高權限操作。若將長效 JWT 或高權限 Token 儲存在 `localStorage`，一旦發生 XSS，攻擊者可透過 JavaScript 讀取並濫用 Token。

但使用 Cookie Authentication 時，瀏覽器會自動帶 Cookie，因此寫入型 API 必須處理 CSRF 風險。

## 2. 決策

正式瀏覽器登入狀態以 HttpOnly Secure Cookie 為主，短期 Token 僅用於受控 API 場景。同時，所有需要登入且會改變資料的 API，必須規劃 CSRF 防護。

Cookie 原則：

```text
HttpOnly = true
Secure = true
SameSite = Lax / Strict；跨子網域或跨站需求時才評估 None + Secure
不把高權限長效 Token 放 localStorage
```

CSRF 原則：

```text
GET / HEAD / OPTIONS 不改變資料
POST / PUT / PATCH / DELETE 需 CSRF Token 或等效防護
前端 API Client 需帶 X-CSRF-TOKEN 類型 Header
後端驗證 Token、Origin / Referer 與 CORS 來源
```

## 3. 理由

- HttpOnly 可降低 Token 被 JavaScript 讀取的風險。
- Secure 確保 Cookie 僅透過 HTTPS 傳送。
- CSRF 防護可降低瀏覽器自動帶 Cookie 被跨站濫用的風險。
- 前後台都涉及高價值資料，不能只靠登入判斷。

## 4. 實作要求

```text
□ Login 成功後 Set-Cookie 使用 HttpOnly + Secure
□ Logout 必須清除 Cookie 或使登入狀態失效
□ 寫入型 API 檢查 CSRF Token
□ 前端 Axios / Fetch 使用 withCredentials
□ 正式 CORS 僅允許指定網域
□ 不允許 AllowAnyOrigin 搭配 Credentials
□ 後台管理員建議逐步加入 MFA
□ 權限異動後需考慮既有 Cookie 有效期間
```

## 5. 影響與取捨

| 面向 | 影響 |
|---|---|
| 優點 | 降低 XSS 竊取 Token 與 CSRF 濫用風險 |
| 成本 | 前端需處理 CSRF Token 流程 |
| 風險 | 若跨網域 Cookie、CORS、SameSite 設定不一致，登入可能失效 |

## 6. 驗收檢查

```text
□ JavaScript 讀不到登入 Cookie
□ HTTP 非 HTTPS 不傳送正式 Cookie
□ 未帶 CSRF Token 的寫入型 API 被拒絕
□ 來源不在白名單的跨站請求被拒絕
□ Cookie 過期或登出後無法呼叫受保護 API
```
