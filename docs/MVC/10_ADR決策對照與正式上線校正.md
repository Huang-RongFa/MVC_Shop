# 10｜ADR 決策對照與正式上線校正

## 1. 本章目的

本章用來把 `docs/adr` 中已確認的架構決策，對照到 MVC、API、前端、資安與正式部署文件，避免文件之間出現不同說法。

如果新文件與 ADR 衝突，應先更新或新增 ADR，而不是在文件中寫出另一套架構。

---

## 2. 已確認決策

| ADR | 決策 | 文件校正方向 |
|---|---|---|
| ADR-0001 | 完整前台電商 + 後台管理系統 | 文件不可只描述後台 CRUD |
| ADR-0002 | 共用 `Users`，用 `UserType` 區分身分 | 帳戶文件需分清 UserType、Role、Permission |
| ADR-0003 | 下單保留庫存，出貨才扣實際庫存 | 訂單、庫存、出貨流程都要使用此語言 |
| ADR-0004 | 付款成功進入 `Paid`，不自動出貨 | 付款與物流流程不可混成同一步 |
| ADR-0005 | 支援部分退款 | 退款文件與 API 要保留 `RefundItems` |
| ADR-0006 | 優惠券與促銷可疊加，但需記錄折扣來源 | API 與文件需保留 `OrderDiscounts` |
| ADR-0007 | 前台 API 與後台 API 分界 | Controller、Route、DTO 需分 Storefront / Admin |
| ADR-0008 | 正式上線使用 HttpOnly Secure Cookie | 文件不可把 localStorage JWT 當正式主方案 |
| ADR-0009 | 前端拆成 Storefront App 與 Admin App | Vue 文件不可只描述單一 SPA |
| ADR-0010 | 後端採模組化單體 | 不採初期微服務拆分 |
| ADR-0011 | 一套共用 MSSQL，依模組分組 | 不採初期多資料庫拆分 |

---

## 3. 正式上線新增校正方向

| 主題 | 決策 | 文件影響 |
|---|---|---|
| CSRF | Cookie Authentication 必須搭配 CSRF 防護 | 03、04、05、06、07、08、09 |
| CORS | 正式環境只允許指定前台與後台網域 | 03、04、05、07、08 |
| Rate Limiting | 登入、搜尋、報表、Callback 等高風險 API 必須限制 | 03、04、06、07、08 |
| ProblemDetails / 錯誤處理 | 正式環境不暴露 Stack Trace | 03、06、07、08 |
| Health Checks | API 與 DB 需要健康檢查端點 | 03、07、08、09 |
| Swagger | Production 不公開或加授權 | 03、07、08、09 |
| 套件弱點 | NuGet / npm 需納入部署前檢查 | 07、08、09 |
| 機密管理 | Secret 不可進 Git，正式環境使用環境變數或 Secret Manager | 04、07、08、09 |
| 備份復原 | 上線前備份，定期測試還原 | 01、07、08 |
| 冪等處理 | 金流 Callback、退款、出貨等需可安全重送 | 02、06、07、08 |
| 監控告警 | 錯誤、登入失敗、付款失敗、資源耗盡需可追蹤 | 07、08 |

---

## 4. 文件對照

| 文件 | 主要責任 |
|---|---|
| `00_總覽與正式上線架構.md` | 系統整體架構、正式部署目標與信任邊界 |
| `01_資料庫與EFCore正式對應.md` | MSSQL、Entity、DbContext、交易、併發與備份 |
| `02_後端分層Repository與Service.md` | Controller、Service、Repository、DTO、Transaction 分工 |
| `03_Program與Middleware安全管線.md` | Program.cs、Middleware、CORS、CSRF、Rate Limiting、Health Checks |
| `04_登入驗證Cookie權限與CSRF安全.md` | Cookie、CSRF、登入、登出、權限與 MFA |
| `05_前台與後台Vue正式部署流程.md` | Storefront / Admin 前端流程、API Client、CSRF、XSS、部署 |
| `06_RESTfulAPI前後台API與交易流程.md` | API 邊界、路由、狀態碼、錯誤格式、交易流程 |
| `07_OWASP與API資安檢查清單.md` | OWASP、API 安全、CORS、CSRF、供應鏈、備份、監控 |
| `08_完整系統開發部署流程清單.md` | 從開發到 Staging / Production 的流程 |
| `09_常用指令與安全檔案模板.md` | 指令、User Secrets、弱點掃描、Build、部署檢查 |
| `10_ADR決策對照與正式上線校正.md` | ADR 與文件一致性校正 |

---

## 5. 不可再出現的衝突寫法

```text
錯誤：正式登入主要使用 localStorage JWT
正確：正式瀏覽器登入以 HttpOnly Secure Cookie 為主，短期 Token 僅作補充

錯誤：前台與後台共用同一組 Controller 與 DTO
正確：前台 API 與後台 API 分界，DTO 也分界

錯誤：付款成功後自動出貨並扣庫存
正確：付款成功只進入 Paid，出貨確認時才扣庫存

錯誤：訂單建立只新增 Orders
正確：建立 Orders、OrderItems、InventoryReservations、OrderDiscounts 與狀態歷史

錯誤：靠前端隱藏按鈕控權
正確：後端 Controller + Service 皆需檢查授權

錯誤：正式環境 AllowAnyOrigin
正確：正式環境只允許指定前台與後台網域

錯誤：Cookie Authentication 不做 CSRF
正確：Cookie 驗證的寫入型 API 必須規劃 CSRF 防護
```

---

## 6. 新增 ADR 建議

若後續要正式推進，建議新增：

```text
ADR-0012：正式部署環境分離策略
ADR-0013：Cookie + CSRF 防護策略
ADR-0014：Rate Limiting 與登入防暴力破解策略
ADR-0015：金流 Callback 驗簽與冪等策略
ADR-0016：正式環境 Swagger 管控策略
ADR-0017：資料庫備份與還原策略
ADR-0018：Health Check、Logging、Monitoring 策略
ADR-0019：前台 SEO 與部署模式策略
ADR-0020：圖片上傳與檔案安全策略
```

---

## 7. 文件維護規則

```text
□ 新增功能文件前先看 ADR
□ 修改交易流程前先看訂單、付款、庫存、退款 ADR
□ 修改登入方式前先看 Cookie 與 CSRF ADR
□ 修改前端架構前先看 Storefront / Admin 分離 ADR
□ 修改資料庫前先看 MSSQL 文件與 Migration / Script 策略
□ 若文件和 ADR 不一致，先修 ADR 或新增 ADR
```

---

## 8. 本章總結

ADR 是專案的架構契約。正式部署後，最怕文件各講各話，導致程式碼、資料庫、前端、API 與資安策略彼此矛盾。後續所有文件都應遵守既有 ADR，若要改變決策，先新增或更新 ADR，再調整程式與文件。
