# 正式上線技術規範總覽

> 版本：2026-06-02  
> 適用專案：ASP.NET Core Web API 模組化單體 + 共用 MSSQL + Storefront Vue App + Admin Vue App  
> 文件定位：正式部署、正式啟用、正式營運前後的技術與資安規範

---

## 1. 文件目的

本資料夾用來補齊電商網站從「能開發」走向「可正式營運」所需的技術規範。正式上線不是把網站丟到伺服器即可，而是要同時確認：

```text
功能可用
安全可控
部署可回滾
資料可備份還原
問題可監控告警
事故可追蹤處理
```

本專案已有資料庫、MVC / Web API、前後台 Vue、ADR 與資安檢查文件；本資料夾負責把這些文件落實成正式上線檢查標準。

---

## 2. 正式上線最低門檻

網站可以開始對外啟用前，至少要完成下列項目：

```text
□ Production 與 Development / Staging 完全分離
□ 正式資料庫與測試資料庫分離
□ 正式金流與測試金流分離
□ 正式物流與測試物流分離
□ HTTPS 全站啟用
□ Cookie 設定 HttpOnly / Secure / SameSite
□ 使用 Cookie 驗證時，寫入型 API 已規劃 CSRF 防護
□ CORS 只允許正式可信任網域
□ 登入、註冊、忘記密碼、優惠券驗證、搜尋等 API 已套用 Rate Limiting
□ Swagger / OpenAPI 正式環境關閉或受保護
□ 錯誤訊息不顯示 Stack Trace、SQL、檔案路徑或機密
□ 重要 Log 可查詢，且不記錄密碼、完整 Token、完整 Cookie、卡片敏感資料
□ 資料庫備份已啟用並完成至少一次還原測試
□ 部署失敗時有回滾方案
□ 金流 Callback 已驗簽並具備冪等處理
□ Health Check 可確認 API 與資料庫狀態
□ 上線前、中、後的驗收清單已執行
```

---

## 3. 文件目錄與使用順序

| 順序 | 文件 | 用途 |
|---|---|---|
| 1 | `00_正式上線整體檢查總表.md` | 總覽全部上線門檻 |
| 2 | `01_網路架構DNS與TLS規範.md` | 網域、DNS、HTTPS、HSTS、WAF、反向代理 |
| 3 | `02_環境分離組態與機密管理規範.md` | Dev / Staging / Production、Secret、權限最小化 |
| 4 | `03_API安全與前後端通訊規範.md` | CORS、Cookie、CSRF、Rate Limit、API 安全 |
| 5 | `04_CICD部署回滾與版本管理規範.md` | 建置、測試、部署、回滾、資料庫變更 |
| 6 | `05_監控日誌告警與稽核規範.md` | Log、Metric、Trace、Alert、Audit |
| 7 | `06_資料保護備份還原與災難復原規範.md` | 備份、還原、RPO、RTO、個資保存 |
| 8 | `07_效能壓測快取與容量規劃規範.md` | 壓測、快取、容量、DB 效能 |
| 9 | `08_金流物流第三方服務與合規規範.md` | 金流、物流、Webhook、退款、敏感資料 |
| 10 | `09_前端正式上線SEO可用性與瀏覽器規範.md` | Vue Build、SEO、可用性、前端安全 |
| 11 | `10_正式上線驗收與營運檢查清單.md` | 上線前一天、上線當下、上線後檢查 |

---

## 4. 與 ADR / MVC 文件的關係

正式上線文件必須遵守既有 ADR：

```text
ADR-0001：完整前台電商與後台管理系統
ADR-0007：分離前台 API 與後台 API 邊界
ADR-0008：正式環境使用 HttpOnly Secure Cookie 並搭配 CSRF 防護
ADR-0009：拆分 Storefront 與 Admin 兩個 Vue App
ADR-0010：後端採用模組化單體架構
ADR-0011：共用 MSSQL 資料庫並依模組分組
ADR-0012～0020：正式部署、CORS、Rate Limiting、Callback、Swagger、備份、監控、SEO、檔案安全
```

若本資料夾與 ADR 衝突，應優先修正 ADR 或新增 ADR，不應直接讓文件出現兩套決策。

---

## 5. 分階段導入建議

### 第一階段：可以安全對外測試

```text
□ HTTPS
□ CORS 白名單
□ Cookie + CSRF
□ Production Secret 不進 Git
□ Swagger 保護
□ 基本 Log
□ 基本備份
□ Smoke Test
```

### 第二階段：可以正式啟用

```text
□ Rate Limiting
□ Health Check
□ 監控告警
□ 金流 Callback 驗簽與冪等
□ 備份還原演練
□ CI/CD 與回滾
□ 權限與稽核檢查
```

### 第三階段：穩定營運

```text
□ 壓測與容量規劃
□ WAF / CDN
□ 集中式 Log / Trace
□ 事故演練
□ 資料保存與匿名化流程
□ 套件弱點定期掃描
```

---

## 6. 本章總結

這份正式上線規範的目標不是保證網站永遠不被攻擊，而是讓系統具備基本防護、承載能力、事故追蹤與復原能力。正式啟用前，至少要完成安全、部署、備份、監控與金流物流檢查。

---

## 參考基準

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- OWASP API Security Top 10 2023: https://owasp.org/API-Security/editions/2023/en/0x11-t10/
- OWASP CSRF Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Microsoft ASP.NET Core CORS: https://learn.microsoft.com/aspnet/core/security/cors
- Microsoft ASP.NET Core Anti-forgery: https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- Microsoft ASP.NET Core Rate Limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Microsoft ASP.NET Core Health Checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
