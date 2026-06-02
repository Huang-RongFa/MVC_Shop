# 04｜登入驗證、Cookie、權限與 CSRF 安全

## 1. 本章目的

本章規劃正式上線的登入、Cookie、短期 Token、CSRF、角色與權限。正式系統應以 HttpOnly Secure Cookie 作為瀏覽器登入狀態，JWT 或短期 Token 只用於受控 API 場景，不建議讓前端 JavaScript 長期保存高權限 Token。

相關資料表：

```text
Users
Roles
UserRoles
Permissions
RolePermissions
UserLoginLogs
AdminActionLogs
```

---

## 2. 正式登入流程

```text
1. Storefront App 或 Admin App 送出帳號密碼
2. Auth Controller 接收 Login Request
3. AuthService 查詢 Users
4. 驗證帳號狀態：啟用、未鎖定、未刪除
5. 驗證 PasswordHash
6. 查詢 UserType、Roles、Permissions
7. 建立 ClaimsPrincipal
8. 後端 Set-Cookie，設定 HttpOnly + Secure + SameSite
9. 寫入 UserLoginLogs
10. 前端呼叫 /auth/me 取得非敏感使用者資訊
11. 後續 API 由瀏覽器自動帶 Cookie
12. 後端依 Cookie、Role、Permission 與資料擁有者授權
```

---

## 3. Cookie 設定規劃

正式環境 Cookie 建議：

```text
HttpOnly = true
Secure = true
SameSite = Lax 或 Strict；跨站情境才評估 None
ExpireTimeSpan = 依風險設定
SlidingExpiration = 依安全需求評估
CookieName = __Host-MySystemAuth 或明確前綴
```

注意：

```text
□ HttpOnly 只能防止 JavaScript 讀 Cookie，不能防止 CSRF
□ Secure 代表只透過 HTTPS 傳送
□ SameSite 可降低部分 CSRF 風險，但不能取代 CSRF Token
□ Cookie Domain、前台網域、後台網域、API 網域要一致規劃
```

---

## 4. 前後台網域建議

建議正式網域：

```text
前台：https://www.example.com
後台：https://admin.example.com
API：https://api.example.com
```

需要確認：

```text
□ Cookie Domain 是否需要跨子網域
□ SameSite 是否會影響跨站請求
□ CORS 是否允許正確來源
□ 前端是否設定 withCredentials / credentials: include
□ CSRF Token 是否可被前端取得並送回 Header
```

若能簡化部署，初期可考慮同站部署：

```text
https://www.example.com
https://www.example.com/admin
https://www.example.com/api
```

同站部署 Cookie 與 CSRF 設定較單純，但前後台隔離與部署彈性較低。

---

## 5. CSRF 防護

Cookie Authentication 的風險是：瀏覽器會自動把 Cookie 帶到符合條件的請求中，因此攻擊者可能誘導使用者瀏覽惡意網站，間接發出寫入型請求。

建議做法：

```text
1. 後端提供取得 CSRF Token 的端點
2. 後端設定一個非 HttpOnly 的 XSRF Token Cookie，讓前端可讀取 token 值
3. 前端將 token 放入 X-CSRF-TOKEN 或 X-XSRF-TOKEN Header
4. 後端對 POST / PUT / PATCH / DELETE 驗證 token
5. GET 僅做查詢，不修改資料
```

需要保護的 API：

```text
□ 登出
□ 加入購物車
□ 建立訂單
□ 建立付款
□ 後台新增、修改、刪除商品
□ 後台調整庫存
□ 後台修改訂單狀態
□ 後台建立出貨
□ 後台退款
□ 後台角色與權限修改
```

---

## 6. 短期 Token 使用範圍

短期 Token 可用於：

```text
□ 第三方 API 串接
□ 行動 App 或非瀏覽器 Client
□ 一次性操作
□ 下載檔案的短時間授權
□ 金流或物流特定 API 的暫時憑證
```

不建議：

```text
□ 把高權限 JWT 長期放 localStorage
□ 把管理員 Token 暴露給 JavaScript 長期讀取
□ 把 Cookie / Token 簽章金鑰放到前端
```

---

## 7. Login Request / Response 規劃

Request：

```text
account
password
rememberMe（若需要）
csrfToken（依流程）
```

Response：

```text
userId
account
displayName
userType
roles
permissions
```

不要回傳：

```text
PasswordHash
SecurityStamp
完整 User Entity
Cookie / Token 簽章金鑰
內部稽核欄位
```

---

## 8. 密碼安全

禁止：

```text
SHA256(password)
MD5(password)
Base64(password)
明文密碼
```

建議：

```text
ASP.NET Core Identity PasswordHasher
PBKDF2
bcrypt
Argon2
```

登入安全控制：

```text
□ 登入失敗寫入 UserLoginLogs
□ 多次失敗鎖定帳號或延遲回應
□ 後台登入加更嚴格 Rate Limiting
□ 後台管理員規劃 MFA
□ 密碼重設使用一次性 Token 並設定短效期
□ 密碼重設成功後使舊登入狀態失效
```

---

## 9. 角色與權限規劃

資料表關係：

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

`UserType` 用來區分使用者類型：

```text
Customer：前台會員
Admin：後台管理員
Staff：一般員工
```

`Roles` 與 `Permissions` 用來描述功能權限：

```text
Products.Read
Products.Write
Inventory.Adjust
Orders.Read
Orders.Manage
Payments.Read
Shipments.Manage
Refunds.Manage
Coupons.Manage
Users.Manage
Roles.Manage
AuditLogs.Read
```

---

## 10. API 權限套用方式

基本登入保護：

```csharp
[Authorize]
```

角色限制：

```csharp
[Authorize(Roles = "Admin")]
```

權限限制建議使用 Policy：

```text
Products.Write
Orders.Read
AuditLogs.Read
```

前台 API：

```text
□ 是否登入
□ 是否查詢自己的資料
□ 是否操作自己的購物車與訂單
```

後台 API：

```text
□ 是否為後台使用者
□ 是否具備 Role
□ 是否具備 Permission
□ 是否符合該資料狀態可操作條件
```

---

## 11. 登出與登入狀態失效

登出不能只讓前端清空狀態。

需要：

```text
□ 後端呼叫 SignOut 清除 Cookie
□ 清除或失效伺服器端登入狀態
□ 前端清空 authStore
□ 登出 API 也需 CSRF 防護
```

權限異動後要考慮：

```text
□ 已登入使用者是否要重新登入
□ Claims 是否要即時刷新
□ 高權限異動是否要強制登出
```

---

## 12. 登入與權限檢查清單

```text
□ Users 保存 PasswordHash，不保存明文密碼
□ 密碼使用安全雜湊
□ Cookie 設定 HttpOnly
□ Cookie 設定 Secure
□ SameSite 與 Domain 已依部署架構規劃
□ 寫入型 API 有 CSRF 防護
□ CORS 僅允許可信任網域
□ 不把高權限 Token 長期放 localStorage
□ 登入成功與失敗寫入 UserLoginLogs
□ 後台登入有 Rate Limiting / Lockout
□ 前台 API 檢查本人資料
□ 後台 API 檢查 Role / Permission
□ 後台管理員規劃 MFA
□ 登出由後端清除 Cookie
□ 權限異動後有登入狀態刷新或失效策略
```

---

## 13. 本章總結

登入不只是產生 Cookie 或 Token，而是完整的身分驗證、CSRF 防護、權限控管、登入紀錄與狀態失效流程。正式上線應以 HttpOnly Secure Cookie 為主，搭配 CSRF Token、CORS 白名單、Rate Limiting、Role / Permission 與稽核紀錄。
