# 04｜CI/CD、部署回滾與版本管理規範

> 版本：2026-06-02  
> 文件目的：讓正式部署可追蹤、可驗證、可回滾，避免每次上線都靠手動操作與臨場判斷。

---

## 1. 本章目的

正式上線後，每次修改都可能影響使用者下單、付款與後台操作。部署流程要可追蹤、可驗證、可回滾。

---

## 2. 分支與版本

建議至少區分：

```text
main：正式穩定版本
develop：開發整合版本
feature/*：功能開發
hotfix/*：正式環境緊急修正
release/*：上線候選版本
```

版本命名可使用 Semantic Versioning：

```text
v1.0.0：正式第一版
v1.0.1：修補錯誤
v1.1.0：新增相容功能
v2.0.0：破壞性變更
```

每次 Production 部署都應對應 Git Tag、建置 Artifact 與部署紀錄。

---

## 3. CI 檢查

每次合併前建議自動執行：

```text
□ dotnet restore
□ dotnet build
□ dotnet test
□ npm ci
□ npm run build
□ Lint
□ 套件弱點掃描
□ Secret 掃描
□ 單元測試
□ API 測試
□ Docker Image 掃描（若使用容器）
```

若 CI 未通過，不應部署到 Staging 或 Production。

---

## 4. CD 部署流程

建議流程：

```text
1. 合併到指定分支
2. 自動建置後端
3. 自動建置 Storefront App
4. 自動建置 Admin App
5. 部署到 Staging
6. 執行 Smoke Test
7. 人工核准正式部署
8. 部署 Production
9. 執行 Production Smoke Test
10. 上線後觀察 Log、Metric、Trace
```

---

## 5. Artifact 與設定分離

```text
□ 後端 Artifact 不包含 Production Secret
□ 前端 Build 不包含敏感金鑰
□ 同一版 Artifact 可部署到不同環境，由環境變數決定設定
□ 每次 Artifact 有版本號、Git Commit、Build 時間
□ 保留上一版可回滾 Artifact
```

---

## 6. 資料庫變更

資料庫變更要特別小心。

檢查：

```text
□ Schema 變更有 SQL Script 或 Migration
□ 變更可重複執行或有保護
□ 先備份再改正式資料庫
□ 大資料表變更先評估鎖表風險
□ 有回滾 SQL 或修復方案
□ 新欄位先允許 Null 或提供 Default，避免舊程式立即壞掉
□ 刪欄位、改型別、改約束需分階段處理
```

建議採用相容部署：

```text
1. 先部署相容舊程式與新程式的資料庫變更
2. 再部署後端與前端
3. 確認穩定後才清理舊欄位或舊流程
```

---

## 7. 回滾策略

```text
□ 後端保留上一版可部署 Artifact
□ 前端保留上一版 Build 檔案
□ 資料庫變更有還原方案
□ Feature Flag 可關閉新功能
□ 上線失敗時有明確負責人與流程
□ 回滾後執行 Smoke Test
□ 回滾原因寫入部署紀錄
```

注意：程式回滾容易，資料庫回滾困難。凡是不可逆資料庫變更，都必須先評估並在 Staging 演練。

---

## 8. Smoke Test 清單

```text
□ /health/live 正常
□ /health/ready 正常
□ Storefront 首頁正常
□ 商品列表正常
□ 登入 API 正常
□ Admin 登入正常
□ 建立測試訂單流程正常
□ 金流 Callback 測試事件正常
□ Log 無大量錯誤
```

---

## 9. 本章總結

CI/CD 的目的不是追求自動化而已，而是降低上線風險。正式電商系統一定要能知道「這次上線改了什麼、誰核准、如何驗證、失敗怎麼退回」。

---

## 參考基準

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- OWASP API Security Top 10 2023: https://owasp.org/API-Security/editions/2023/en/0x11-t10/
- OWASP CSRF Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Microsoft ASP.NET Core CORS: https://learn.microsoft.com/aspnet/core/security/cors
- Microsoft ASP.NET Core Anti-forgery: https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- Microsoft ASP.NET Core Rate Limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Microsoft ASP.NET Core Health Checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
