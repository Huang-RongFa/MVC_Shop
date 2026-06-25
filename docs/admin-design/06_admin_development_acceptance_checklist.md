# 06｜後台開發順序與驗收清單

## 結論

後台建議不要一次全部重寫，而是依「權限先行、交易核心、行銷模組、營運總覽、正式化驗收」的順序開發。這樣可以避免先做漂亮畫面，最後才發現權限、交易一致性與稽核補不起來。

---

## 1. 建議開發順序

```text
Phase 0：文件與架構對齊
Phase 1：權限與系統基礎
Phase 2：商品與交易核心
Phase 3：行銷與洞察
Phase 4：營運總覽 Dashboard
Phase 5：資安、稽核與正式化驗收
```

---

## 2. Phase 0：文件與架構對齊

目標：先讓 Codex / 開發者知道後台邊界。

工作：

```text
□ 將本文件放入 docs/admin-design/
□ README.md 補上 docs/admin-design 文件導覽
□ CONTEXT.md 補上後台四大模組說明
□ 確認 ADR 不衝突
□ 確認既有 AdminController 與 _AdminLayout 保留
```

驗收：

```text
□ 文件可以解釋每個後台頁面的用途
□ 文件可以解釋每個後台頁面的權限
□ 文件可以解釋 Controller / Service / Repository 分工
```

---

## 3. Phase 1：權限與系統基礎

目標：先建立可控管的後台安全地基。

工作：

```text
□ 確認 AdminOnly Policy
□ 確認所有 Permission Policy
□ 補齊角色與權限 Seed
□ 補齊 Users / Roles / Permissions 後台 Service
□ 補齊 AuditLog / AdminActionLog 寫入機制
□ 補齊後台登入失敗紀錄
□ 補齊 Rate Limiting 設計或 TODO 任務
```

驗收：

```text
□ Customer 無法進入 /admin
□ Staff 只能看到有權限的選單
□ 無權限直接輸入 URL 會 403
□ 指派角色、停用帳號、修改權限會留下紀錄
□ 不允許停用最後一個 SystemAdmin
```

---

## 4. Phase 2：商品與交易核心

目標：完成電商核心營運閉環。

工作：

```text
□ Product / SKU Admin Service
□ Inventory Admin Service
□ Order Admin Service
□ Payment Admin Query Service
□ Shipment Admin Service
□ Refund Admin Service
□ 商品上架檢查
□ 庫存保留與釋放規則
□ 出貨消耗保留庫存規則
□ 部分退款規則
```

驗收：

```text
□ 沒有 Products.Write 不能改商品
□ 商品未補 SKU / 圖片 / 價格不能上架
□ 下單保留庫存，不直接扣實際庫存
□ 付款成功不會自動出貨
□ 出貨確認才消耗保留庫存
□ 退款不可超過可退款金額
□ 改價、調庫存、出貨、退款都有稽核紀錄
```

---

## 5. Phase 3：行銷與洞察

目標：建立折扣可追溯、分析可控管的行銷後台。

工作：

```text
□ Coupon Admin Service
□ Promotion Admin Service
□ Discount calculation source tracking
□ OrderDiscounts 寫入規則
□ Analytics Query Service
□ Notification / Inbox 規則
```

驗收：

```text
□ 優惠券由後端驗證，不由前台決定
□ 促銷與優惠券可疊加，但每筆折扣都有來源
□ Coupons.Manage 才能發布優惠券
□ Promotions.Manage 才能發布促銷
□ Analytics.Read 才能查看營收與報表
□ 行銷活動發布、暫停、結束都有操作紀錄
```

---

## 6. Phase 4：營運總覽 Dashboard

目標：把分散的營運資訊變成管理者首頁。

工作：

```text
□ IAdminDashboardService
□ 今日營收 / 訂單 / 待出貨 / 低庫存指標
□ 待辦事項規則
□ 異常警示規則
□ 最近訂單
□ 最近後台操作
□ 權限感知 Dashboard
```

驗收：

```text
□ Dashboard 依權限顯示不同卡片
□ 沒有 Analytics.Read 不顯示敏感營收資料
□ 待出貨、低庫存、退款待審可以導向正確頁面
□ Dashboard 不直接做高風險操作
□ 不使用假資料作正式數據
```

---

## 7. Phase 5：正式化驗收

目標：讓後台接近正式商業系統。

工作：

```text
□ CSRF 驗證確認
□ Cookie Secure / HttpOnly 檢查
□ API 錯誤格式統一
□ AuditLog 查詢與遮罩
□ 金流 Payload 遮罩
□ 個資欄位遮罩
□ 權限測試案例
□ 交易一致性測試案例
□ 後台頁面 RWD 檢查
```

驗收：

```text
□ API 不會回傳 HTML 登入頁給前端
□ 401 / 403 / 400 / 500 格式一致
□ 高風險 POST / PUT / PATCH / DELETE 都需要 CSRF
□ 正式環境 Cookie 使用 Secure
□ Controller 沒有直接操作 DbContext
□ Entity 不直接回傳前端
□ 重要操作可以從 AuditLog 追溯
```

---

## 8. Codex 任務拆分範本

### 任務 1：建立後台權限與系統文件對齊

```text
請先閱讀 README.md、CONTEXT.md、docs/adr、docs/MVC、docs/MSSQL、docs/admin-design。
請不要破壞現有 MVC / Razor 頁面與 Controller 路由。
請依照 docs/admin-design/01_permissions_and_system.md，檢查目前 AdminOnly、Permission Policy、Seed 與 _AdminLayout.cshtml 是否一致。
只提出修改清單與最小修正，不要重構整個專案。
```

### 任務 2：補商品與交易 Service 介面

```text
請依照 docs/admin-design/02_products_and_transactions.md 與 05_admin_api_service_data_mapping.md，建立後台商品、SKU、庫存、訂單、付款、出貨、退款的 Application Service 介面與 DTO 草稿。
請遵守 Controller 不直接操作 DbContext、Entity 不直接回傳前端、商業邏輯放 Service 的原則。
本次先建立介面與 DTO，不要一次實作所有資料庫邏輯。
```

### 任務 3：補 Dashboard 真實資料來源

```text
請依照 docs/admin-design/04_operations_dashboard.md，將目前 AdminPageService 中 Dashboard 的待串接資料逐步改為透過 IAdminDashboardService 查詢。
請依權限決定顯示卡片，不要讓沒有 Analytics.Read 的角色看到敏感營收資料。
請保留既有 Razor View 結構，只替換資料來源。
```

---

## 9. 最終總驗收清單

### 架構

```text
□ Controller 只做路由、授權、DTO Binding、Response
□ Service 負責商業規則與權限檢查
□ Repository 負責資料存取
□ DTO 與 Entity 分離
□ Admin API 與 Storefront API 分離
```

### 權限

```text
□ AdminOnly 檢查 UserType
□ 每個頁面有對應 Permission Policy
□ 每個高風險 API 有對應 Permission Policy
□ 選單顯示與後端授權一致
□ Service 層再次檢查高風險操作
```

### 交易

```text
□ 訂單建立、庫存保留、付款、出貨、退款流程一致
□ 金流 Callback 驗簽與冪等
□ 出貨才消耗保留庫存
□ 部分退款可追蹤品項
□ 交易失敗會 rollback
```

### 稽核

```text
□ 登入成功 / 失敗有紀錄
□ 高風險操作有 AdminActionLog
□ 重要資料異動有 AuditLog
□ 系統錯誤有 SystemErrorLog
□ 稽核紀錄不可被一般角色查看
```

### UI

```text
□ 後台選單依權限顯示
□ Dashboard 是營運工作台，不是靜態模板
□ 表格可搜尋、篩選、分頁
□ 高風險操作有確認提示
□ 無資料狀態與錯誤狀態清楚
```
