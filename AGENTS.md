# AGENTS.md

## 1. 文件目的

本文件是給 Codex、ChatGPT、Claude Code、Cursor、Windsurf 或其他 AI coding agent 使用的「實作守則」。

AI 在修改本專案任何程式碼、設定、資料庫、前端頁面或文件前，必須先依照本文件對齊專案規則。

本專案不是教學型 CRUD，而是以正式部署、正式營運為目標的完整交易型電商系統。

---

## 2. 回覆語言

除非使用者指定其他語言，所有說明、註解與回覆請使用繁體中文。

程式碼、類別名稱、檔案名稱、Git branch、API route、資料表名稱維持英文命名。

---

## 3. 專案文件讀取順序

修改程式碼前，請依任務類型讀取文件。

### 3.1 基本必讀

```text
1. AGENTS.md
2. CONTEXT.md
3. README.md
```

### 3.2 架構或重大設計必讀

```text
docs/adr/README_ADR決策總覽.md
docs/adr/0001_完整前台電商與後台管理系統.md
docs/adr/0007_分離前台API與後台API邊界.md
docs/adr/0008_正式環境使用HttpOnlySecureCookie並搭配CSRF防護.md
docs/adr/0009_拆分Storefront與Admin兩個VueApp.md
docs/adr/0010_後端採用模組化單體架構.md
docs/adr/0011_共用MSSQL資料庫並依模組分組.md
```

### 3.3 資料庫、Entity、Repository 任務必讀

```text
docs/MSSQL/
docs/MVC/01_資料庫與EFCore正式對應.md
docs/MVC/02_後端分層Repository與Service.md
```

### 3.4 API、Service、Controller 任務必讀

```text
docs/MVC/02_後端分層Repository與Service.md
docs/MVC/03_Program與Middleware安全管線.md
docs/MVC/04_登入驗證Cookie權限與CSRF安全.md
docs/MVC/06_RESTfulAPI前後台API與交易流程.md
docs/MVC/07_OWASP與API資安檢查清單.md
```

### 3.5 前端任務必讀

```text
docs/MVC/05_前台與後台Vue正式部署流程.md
docs/頁面設計/
docs/正式上線技術規範/09_前端正式上線SEO可用性與瀏覽器規範.md
```

### 3.6 正式部署、資安、CI/CD 任務必讀

```text
docs/正式上線技術規範/README_正式上線技術規範總覽.md
docs/正式上線技術規範/00_正式上線整體檢查總表.md
docs/正式上線技術規範/01_網路架構DNS與TLS規範.md
docs/正式上線技術規範/02_環境分離組態與機密管理規範.md
docs/正式上線技術規範/03_API安全與前後端通訊規範.md
docs/正式上線技術規範/04_CICD部署回滾與版本管理規範.md
docs/正式上線技術規範/05_監控日誌告警與稽核規範.md
docs/正式上線技術規範/06_資料保護備份還原與災難復原規範.md
docs/正式上線技術規範/07_效能壓測快取與容量規劃規範.md
docs/正式上線技術規範/08_金流物流第三方服務與合規規範.md
docs/正式上線技術規範/10_正式上線驗收與營運檢查清單.md
```

---

## 4. 文件優先權

若文件內容互相衝突，請依下列優先順序判斷：

```text
1. 使用者本次明確指示
2. docs/adr/*
3. docs/正式上線技術規範/*
4. docs/MSSQL/*
5. docs/MVC/*
6. docs/頁面設計/*
7. CONTEXT.md
8. README.md
```

如果新需求會推翻既有 ADR，不得直接修改程式碼，必須先提出：

```text
□ 衝突的 ADR
□ 為何需要修改
□ 影響範圍
□ 替代方案
□ 回復方案
□ 建議新增或修正的 ADR
```

---

## 5. 專案定位

本專案是完整交易型電商系統，包含：

```text
Storefront Vue App
Admin Vue App
ASP.NET Core Web API 模組化單體
EF Core
共用 MSSQL Database
Cookie Authentication
Role / Permission Authorization
Audit Logs
正式上線技術規範
```

本專案不是：

```text
□ 教學型 CRUD
□ Demo 專案
□ 單表管理系統
□ 只有後台管理的系統
□ 傳統 MVC View-only 網站
□ 初期微服務架構
```

---

## 6. 核心架構不可任意改動

本專案固定採用：

```text
ASP.NET Core Web API 模組化單體
+
共用 MSSQL Database
+
Storefront Vue App
+
Admin Vue App
+
正式環境 HttpOnly Secure Cookie + CSRF 防護
```

禁止未經確認就改成：

```text
□ 微服務優先
□ 多資料庫優先
□ JWT First 並長期放 localStorage
□ 前台與後台共用同一組 Controller
□ Controller 直接操作 DbContext
□ Entity 直接回傳 API
```

---

## 7. 分層責任

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

### Controller

只能負責：

```text
□ 接收 HTTP Request
□ Model Validation
□ 呼叫 Service
□ 回傳 HTTP Status Code
□ 回傳 DTO
```

不得負責：

```text
□ 複雜商業流程
□ 多表交易
□ 直接操作 DbContext
□ 決定庫存是否扣除
□ 決定訂單是否可退款
□ 決定付款成功後的履約流程
```

### Service

負責：

```text
□ 商業規則
□ 權限判斷
□ 交易流程
□ DTO 組裝
□ Transaction 控制
□ 呼叫 Repository
□ 稽核紀錄觸發
```

### Repository

負責：

```text
□ EF Core 查詢
□ 新增、修改、刪除資料
□ 查詢投影
□ 必要的 Include / AsNoTracking / 分頁查詢
```

不得決定商業規則。

### Entity / DTO

```text
Entity：對應資料表，不可直接回傳 API
DTO：API 輸入輸出模型，只包含前端需要的欄位
ViewModel：MVC View 或前端畫面需要的顯示模型
```

---

## 8. API 邊界

前台與後台 API 必須分開。

```text
前台 API：/api/storefront/*
後台 API：/api/admin/*
```

### 前台 API 原則

```text
□ 顧客只能操作自己的資料
□ 商品價格由後端重新查詢
□ 折扣由後端重新計算
□ 不相信前端傳來的角色、權限、價格
```

### 後台 API 原則

```text
□ 必須檢查 Role 或 Permission
□ 高風險操作要寫入 AdminActionLogs 或 AuditLogs
□ 權限不可只靠前端隱藏按鈕
□ 退款、庫存調整、權限異動要特別檢查
```

---

## 9. 正式上線資安底線

任何正式部署相關變更都必須遵守：

```text
□ HTTPS
□ HSTS 規劃
□ CORS 白名單
□ Cookie HttpOnly / Secure / SameSite
□ CSRF 防護
□ Rate Limiting
□ Security Headers
□ ProblemDetails / 全域錯誤處理
□ Swagger 正式環境關閉或加保護
□ Production Secret 不進 Git
□ Log 不記錄密碼、完整 Token、完整 Cookie、信用卡敏感資料
□ Health Check
□ 備份還原演練
□ 監控與告警
```

禁止：

```text
□ Production 使用 AllowAnyOrigin
□ Production 顯示 Stack Trace
□ Production 直接公開 Swagger
□ 把 Connection String、金流金鑰、SMTP 密碼寫進 Git
□ 把高權限 Token 長期放 localStorage
```

---

## 10. 交易流程規則

下列流程必須使用 Service 管理，且需要考慮 Transaction：

```text
□ 建立訂單 + 建立訂單明細 + 保留庫存
□ 取消訂單 + 釋放保留庫存 + 訂單狀態歷史
□ 付款成功 + 寫入金流交易 + 更新訂單狀態
□ 出貨成功 + 扣庫存 + 寫入庫存異動
□ 退款成功 + 寫入退款明細 + 更新付款狀態
□ 使用優惠券 + 寫入使用紀錄 + 寫入訂單折扣
```

核心決策：

```text
□ 下單時保留庫存
□ 出貨時才扣實際庫存
□ 付款成功只進入 Paid
□ 付款成功不自動出貨
□ 支援整筆退款與部分退款
□ 優惠券與促銷可疊加，但必須記錄折扣來源
```

---

## 11. 寫程式前必須先輸出

提供程式碼或修改方案前，必須先輸出：

```text
【目的說明】

【目前判斷】

【參考文件】

【建議做法】

【會修改的檔案】

【是否涉及權限】

【是否涉及 Transaction】

【是否涉及資安】

【風險與替代方案】
```

接著才提供：

```text
【完整程式碼】

【測試方式】

【完成檢查】

【後續建議】
```

---

## 12. 小步修改原則

請遵守：

```text
□ 一次只處理明確範圍
□ 優先最小可行實作
□ 不任意重構無關檔案
□ 不任意新增套件
□ 不任意修改資料庫結構
□ 不任意刪除檔案
□ 不任意改命名空間
□ 不任意改專案架構
```

如果必須做較大修改，先提供「重構計畫」，不要直接改。

---

## 13. TDD 與測試規則

適合先寫測試或測試規格的情境：

```text
□ 訂單建立
□ 庫存保留
□ 優惠券驗證
□ 付款狀態更新
□ 金流 Callback 冪等
□ 退款金額檢查
□ 權限檢查
□ API Response 格式
□ Service 商業規則
```

每個功能完成時至少要說明：

```text
□ 如何用 Swagger / Postman 測
□ 如何用前端畫面測
□ 如何測 401 / 403
□ 如何測錯誤輸入
□ 是否需要測 Transaction rollback
□ 是否需要測 AuditLogs / AdminActionLogs
```

---

## 14. AI 任務拆解格式

遇到較大的需求，請拆成任務清單：

```text
任務 1：資料表與 Entity 對齊
任務 2：Repository 查詢
任務 3：Service 商業規則
任務 4：Controller API
任務 5：DTO 與驗證
任務 6：權限與稽核
任務 7：前端串接
任務 8：測試與文件更新
```

不得一次產生難以審查的大量修改。

---

## 15. 完成定義

一個功能完成，不是「能跑」就算完成。

必須符合：

```text
□ 不違反 ADR
□ Controller / Service / Repository 分層正確
□ DTO 不暴露敏感資料
□ 前台 API 檢查本人資料
□ 後台 API 檢查 Role / Permission
□ 必要流程使用 Transaction
□ EF Core 查詢合理
□ 錯誤處理明確
□ HTTP Status Code 合理
□ Log / Audit 規劃合理
□ 可測試
□ 可回復
□ 文件未衝突
```
