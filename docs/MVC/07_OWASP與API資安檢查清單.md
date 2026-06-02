# 07｜OWASP 與 API 資安檢查清單

## 1. 本章目的

本章依電商正式部署需求整理資安檢查。此系統包含帳戶、商品、庫存、訂單、付款、物流、優惠與稽核資料，所以資安規劃不能只看登入功能。

目標不是保證永遠不被攻擊，而是降低常見攻擊成功率，並在異常發生時能偵測、限制影響、追蹤原因與復原服務。

---

## 2. 權限控制

風險：使用者登入後，可以操作不屬於自己或自己權限範圍的 API。

需要檢查：

```text
□ Controller 加上 [Authorize]
□ 後台管理 API 限制 Admin / Manager / Staff
□ Service 層再次檢查資料權限
□ 前台會員只能查自己的訂單與購物車
□ 後台按鈕隱藏只是 UX，後端仍要檢查權限
□ AuditLogs / AdminActionLogs 只允許授權人員查詢
□ 重要 API 使用 Permission Policy
```

對應資料表：

```text
Users
Roles
UserRoles
Permissions
RolePermissions
AdminActionLogs
```

---

## 3. 身分驗證與登入防護

風險：暴力破解、Cookie 被濫用、登入狀態未失效。

需要檢查：

```text
□ 登入失敗寫入 UserLoginLogs
□ 多次失敗鎖定帳號或延遲回應
□ 登入 API 設 Rate Limiting
□ 後台登入比前台更嚴格
□ 後台管理員規劃 MFA
□ 登入狀態過期後不可繼續使用
□ 登出時後端清除 Cookie 或使登入狀態失效
□ 權限異動後有 Claims 刷新或強制登出策略
```

---

## 4. Cookie、CORS 與 CSRF

風險：惡意網站誘導已登入使用者發出請求，或 CORS 設定過寬造成資料外洩。

需要檢查：

```text
□ Cookie 設定 HttpOnly
□ Cookie 設定 Secure
□ Cookie 設定 SameSite
□ CORS 只允許正式前台與後台網域
□ 不使用 AllowAnyOrigin + AllowCredentials
□ 寫入型 API 驗證 CSRF Token
□ 登出、下單、付款、後台操作都需要 CSRF 防護
□ GET 不做資料異動
```

---

## 5. 密碼與機密保護

風險：密碼或金鑰外洩。

需要檢查：

```text
□ 密碼不使用明文
□ 密碼不使用單純 SHA256 / MD5
□ 使用 PasswordHasher、PBKDF2、bcrypt 或 Argon2
□ Cookie / Token 簽章金鑰不進 Git
□ MSSQL 連線字串不寫死在程式碼
□ 金流與物流 API Key 使用環境變數或 Secret Manager
□ Production Secret 不放在 appsettings.json
□ CI/CD Secret 不印出到 Log
```

---

## 6. SQL Injection 與查詢濫用

風險：使用者輸入被拼進 SQL，造成資料被竊取或破壞。

建議：

```text
□ 優先使用 EF Core LINQ
□ 使用參數化查詢
□ 不使用字串拼接 SQL
□ 查詢條件需要白名單限制
□ 排序欄位不可直接相信前端輸入
□ 列表查詢一定要分頁
□ 報表查詢限制日期範圍
```

容易發生的地方：

```text
商品關鍵字搜尋
訂單篩選
稽核紀錄查詢
報表查詢
後台排序欄位
```

---

## 7. 不安全設計

風險：流程本身沒有防呆，即使程式沒有錯也會產生錯誤資料。

需要檢查：

```text
□ 訂單狀態不能任意跳轉
□ 付款成功才能進入後續出貨流程
□ 付款成功不自動出貨
□ 出貨時才正式扣實際庫存
□ 退款不能超過可退款金額
□ 優惠券不能超過使用限制
□ 商品價格不能由前端決定
□ 稽核紀錄不可被一般後台人員刪除
□ 金流 Callback 必須驗簽與冪等處理
```

---

## 8. 安全設定錯誤

風險：開發環境設定被帶到正式環境。

需要檢查：

```text
□ 正式環境關閉詳細錯誤訊息
□ Swagger 不公開，或加上保護
□ CORS 不使用 AllowAnyOrigin
□ HTTPS 啟用
□ HSTS 依部署架構規劃
□ Cookie / Token 簽章金鑰長度足夠
□ Cookie / Token 有效期限合理
□ appsettings.Production.json 不上傳公開 repo
□ 預設帳密已移除或停用
□ 資料庫帳號採最小權限
```

---

## 9. 元件與供應鏈風險

需要檢查：

```text
□ 定期更新 NuGet 套件
□ 定期更新 npm 套件
□ 執行 NuGet vulnerability audit
□ 執行 npm audit
□ 移除不使用的套件
□ 前端套件來源可信
□ 不安裝來路不明套件
□ 部署前產生套件弱點報告
```

---

## 10. 資料完整性

風險：訂單、付款、庫存等資料只寫入一部分。

需要 Transaction 的流程：

```text
□ 建立訂單 + 訂單明細 + 保留庫存
□ 取消訂單 + 釋放保留庫存 + 訂單狀態歷史
□ 付款成功 + 金流交易 + 訂單狀態
□ 出貨成功 + 扣庫存 + 庫存異動
□ 退款成功 + 退款明細 + 付款狀態
□ 使用優惠券 + 使用紀錄 + 訂單折扣
```

需要冪等的流程：

```text
□ 金流 Callback
□ 退款 Callback
□ 重複送出付款
□ 重複確認出貨
□ 重複使用優惠券
```

---

## 11. 日誌、稽核與監控

對應資料表：

```text
AuditLogs
AdminActionLogs
SystemErrorLogs
UserLoginLogs
```

建議紀錄：

```text
□ 誰登入
□ 誰新增、修改、刪除商品
□ 誰調整庫存
□ 誰修改訂單狀態
□ 誰處理退款
□ 誰修改角色與權限
□ 系統錯誤發生時間與原因
□ 金流 Callback 處理結果
```

不要紀錄：

```text
完整密碼
完整 Token
完整信用卡號
Cookie 值
金流密鑰
Connection String
個資過度蒐集
```

---

## 12. 前後端分離資安重點

```text
□ 前端不能保存任何後端機密
□ 前端傳來的價格、角色、權限都要重新驗證
□ API 回傳 DTO，不直接回傳 Entity
□ 錯誤訊息不要暴露 SQL、檔案路徑、Stack Trace
□ CORS 只允許可信任網域
□ 重要操作需要後端再次檢查權限
□ 避免 v-html 顯示未清理資料
□ 上傳檔案限制類型、大小與儲存位置
```

---

## 13. 防止網站被流量打垮

一般 ASP.NET Core Rate Limiting 只能降低濫用，不能單獨抵抗大型 DDoS。正式部署需要多層保護。

應規劃：

```text
□ API Rate Limiting
□ 登入 Lockout
□ 查詢分頁上限
□ 報表日期範圍限制
□ 上傳檔案大小限制
□ 反向代理或雲端防火牆
□ CDN / WAF 規劃
□ Health Check 與自動重啟
□ 資料庫連線池與查詢效能監控
□ Log 異常告警
```

---

## 14. 備份與復原

```text
□ 資料庫定期備份
□ 上線前手動備份
□ 備份檔加密或限制存取
□ 定期測試還原
□ 部署失敗可回滾
□ 有事故處理流程
□ 有管理員帳號復原流程
```

---

## 15. 部署前資安檢查清單

```text
□ HTTPS 正常
□ CORS 僅允許正式網域
□ Cookie HttpOnly / Secure / SameSite 正確
□ CSRF Token 對寫入型 API 生效
□ 登入 Rate Limit 生效
□ 前台會員無法查別人的訂單
□ 後台無權限者不能呼叫管理 API
□ 商品價格由後端重新計算
□ 訂單流程使用 Transaction
□ 金流 Callback 驗簽與冪等
□ Swagger 未對外公開
□ Production 不顯示詳細錯誤
□ NuGet / npm 弱點檢查完成
□ 資料庫已備份並測試還原
□ Health Check 可用但不洩漏敏感資訊
□ Log 與告警可追蹤錯誤
```

---

## 16. 本章總結

資安不是最後才補上的功能，而是每個模組都要一起規劃。此系統特別要注意權限、Cookie + CSRF、交易一致性、付款物流資料、庫存異動、API 防濫用、供應鏈弱點、備份復原與稽核紀錄。
