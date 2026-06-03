# 01｜網路架構、DNS 與 TLS 規範

> 版本：2026-06-02  
> 文件目的：確保正式網站的網路入口、網域、HTTPS、反向代理、防火牆與資料庫隔離符合正式營運需求。

---

## 1. 本章目的

電商網站正式上線後，使用者會透過網域進入前端，再由前端呼叫後端 API。網路架構要能保護傳輸安全、隔離內部資源，並讓服務穩定對外。

---

## 2. 建議網路拓樸

```text
使用者瀏覽器
    ↓ HTTPS
CDN / WAF
    ↓ HTTPS
Storefront App / Admin App
    ↓ HTTPS API
反向代理 / Load Balancer
    ↓ 私有網路或內部連線
ASP.NET Core Web API
    ↓ 私有網路
MSSQL Database
```

初學者可以先理解：

```text
CDN：讓前端靜態檔案與圖片更快
WAF：阻擋常見 Web 攻擊與惡意流量
Load Balancer：分流請求並支援健康檢查
反向代理：統一後端入口、TLS 終止、轉發標頭
私有網路：資料庫不可直接暴露到網際網路
```

---

## 3. DNS 規劃

建議網域：

```text
www.example.com       前台網站
admin.example.com     後台管理
api.example.com       後端 API
```

DNS 檢查：

```text
□ A / CNAME 記錄正確
□ www 與裸網域轉址規劃完成
□ admin 後台不與前台混在同一入口
□ api 網域只提供 API
□ Storefront 與 Admin 可以分開設定快取、CORS 與安全政策
□ DNS TTL 設定合理，正式切換前可先降低 TTL
□ DNS 服務帳號啟用 MFA
□ DNS 修改權限限制在必要人員
```

---

## 4. TLS / HTTPS 規範

正式環境必須使用 HTTPS。

檢查項目：

```text
□ TLS 憑證有效
□ 憑證自動更新機制已確認
□ HTTP 自動轉 HTTPS
□ 不允許舊版 TLS
□ API、前台、後台都使用 HTTPS
□ Cookie 設定 Secure
□ 反向代理正確轉發 X-Forwarded-Proto / X-Forwarded-For
□ 後端已正確設定 Forwarded Headers，避免錯判 Scheme 與 IP
```

---

## 5. HSTS 規劃

HSTS 可以要求瀏覽器之後只用 HTTPS 連線。

建議流程：

```text
1. 先確認全站 HTTPS 正常
2. Staging 測試 HSTS
3. Production 先使用較短 max-age
4. 確認無問題後再拉長時間
5. 慎重評估是否加入 preload
```

注意：HSTS 設錯可能導致使用者一段時間內無法用 HTTP 回退，所以要先測試。

---

## 6. 防火牆與連線限制

```text
□ MSSQL 不直接對外開放
□ 後端 API 只開必要 Port
□ 管理後台可限制 IP、VPN 或加強 MFA
□ SSH / RDP 不開放給全網
□ 金流與物流 Webhook 來源可驗證，但不能只依賴 IP 白名單
□ 後端到資料庫只允許必要來源
□ 伺服器管理帳號使用金鑰或 MFA，避免弱密碼登入
```

---

## 7. WAF / CDN 建議

正式啟用後，建議逐步加入 WAF 與 CDN：

```text
□ 常見惡意 Request 阻擋
□ SQL Injection / XSS 基本規則
□ DDoS 基本防護
□ 靜態資源快取
□ 圖片快取與壓縮
□ API 不隨意快取個人資料、訂單、付款狀態
```

WAF 不應取代後端驗證。後端仍必須自行做授權、輸入驗證、CSRF、Rate Limiting 與交易規則檢查。

---

## 8. 本章總結

正式上線時，資料庫應在內部網路，外部使用者只透過 HTTPS 存取前端與 API。DNS、TLS、HSTS、防火牆、反向代理、WAF 與 CDN 都要納入規劃，才算具備基本網路安全。

---

## 參考基準

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- OWASP API Security Top 10 2023: https://owasp.org/API-Security/editions/2023/en/0x11-t10/
- OWASP CSRF Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Microsoft ASP.NET Core CORS: https://learn.microsoft.com/aspnet/core/security/cors
- Microsoft ASP.NET Core Anti-forgery: https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- Microsoft ASP.NET Core Rate Limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Microsoft ASP.NET Core Health Checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
