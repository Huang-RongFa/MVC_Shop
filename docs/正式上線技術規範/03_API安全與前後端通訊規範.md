# 03｜API 安全與前後端通訊規範

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
□ 管理 API 加上 Role 或 Permission
□ 前台 API 檢查資料擁有者
□ 後台 API 檢查管理權限
□ 不直接回傳 Entity
□ 使用 DTO 控制輸入與輸出
□ 輸入資料在後端驗證
□ 錯誤訊息不暴露 Stack Trace
□ 大量資料查詢使用分頁
```

---

## 3. CORS 規範

正式環境只允許正式前端網域。

```text
允許：
https://www.example.com
https://admin.example.com

不建議：
AllowAnyOrigin
```

正式上線使用 HttpOnly Secure Cookie 時，還要確認：

```text
□ Cookie Secure
□ Cookie HttpOnly
□ SameSite 設定
□ CSRF 防護
□ Axios / Fetch 使用 withCredentials
□ 不在 localStorage 長期保存高權限 Token
```

---

## 4. Rate Limit

建議限制高風險 API：

```text
□ 登入 API
□ 註冊 API
□ 忘記密碼 API
□ 優惠券驗證 API
□ 金流 Callback API
□ 搜尋 API
```

Rate Limit 的目的是避免暴力破解、惡意刷 API 或拖垮系統。

---

## 5. Security Headers

正式環境建議設定：

```text
Strict-Transport-Security
X-Content-Type-Options
X-Frame-Options 或 Content-Security-Policy frame-ancestors
Referrer-Policy
Content-Security-Policy
Permissions-Policy
```

初學者可以先理解：Security Headers 是瀏覽器層的保護設定，用來降低 XSS、點擊劫持與不安全資源載入的風險。

---

## 6. API 版本規劃

正式上線後，API 不應隨意破壞前端或 App。

建議：

```text
/api/v1/storefront/products
/api/v1/admin/orders
```

或至少在文件中標記 API 版本與變更紀錄。

---

## 7. Webhook 安全

金流、物流常會透過 Webhook 通知系統。

檢查：

```text
□ 驗證簽章
□ 驗證時間戳避免重放攻擊
□ 記錄 Webhook 原始事件 ID
□ 同一事件重送時不可重複扣款或重複出貨
□ Webhook 錯誤要可重試
```

---

## 8. 本章總結

前端只負責操作體驗，不能被當作安全邊界。API 必須自行完成驗證、授權、輸入檢查、錯誤處理、Rate Limit 與重要流程保護，且前台 API 與後台 API 必須維持清楚邊界。
