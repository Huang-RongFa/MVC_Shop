# 04｜營運總覽設計

## 結論

營運總覽應作為後台首頁，重點不是放漂亮圖表，而是讓管理者快速知道「今天發生什麼事、哪裡需要處理、哪個模組有異常」。Dashboard 只應顯示摘要與導向，不應直接執行高風險異動。

---

## 1. Dashboard 定位

Dashboard 是營運工作台，不是單純報表頁。

應回答四個問題：

```text
□ 今天賣得如何？
□ 有多少訂單需要處理？
□ 有哪些庫存、付款、出貨、退款異常？
□ 哪些操作需要管理者注意？
```

目前對應頁面：

```text
Views/Admin/Index.cshtml
AdminController.Index
IAdminPageService.GetDashboardAsync
Dashboard.Read
```

---

## 2. 首頁資訊架構

建議 Dashboard 區塊：

```text
1. 營運指標卡片
2. 待處理事項
3. 異常警示
4. 最近訂單
5. 低庫存 SKU
6. 付款與退款摘要
7. 最近後台操作
8. 快速入口
```

頁面草圖：

```text
┌──────────────────────────────────────────────┐
│ FreshMart Admin Dashboard                     │
├────────────┬────────────┬────────────┬────────┤
│ 今日營收   │ 今日訂單   │ 待出貨     │ 低庫存 │
├───────────────────────┬──────────────────────┤
│ 待處理事項             │ 異常警示              │
├───────────────────────┴──────────────────────┤
│ 最近訂單 / 付款 / 退款                         │
├──────────────────────────────────────────────┤
│ 最近後台操作 / 快速入口                         │
└──────────────────────────────────────────────┘
```

---

## 3. 營運指標卡片

| 卡片 | 資料來源 | 權限 | 點擊導向 |
|---|---|---|---|
| 今日營收 | Orders, Payments | `Dashboard.Read` + `Analytics.Read` | Analytics |
| 今日訂單 | Orders | `Dashboard.Read` + `Orders.Read` | Orders |
| 待付款 | Orders, Payments | `Orders.Read` | Orders |
| 待出貨 | Orders, Shipments | `Shipments.Manage` | Shipments |
| 退款申請 | Refunds | `Refunds.Manage` | Refunds |
| 低庫存 SKU | InventoryStocks | `Inventory.Read` | Inventory |
| 付款失敗 | Payments | `Payments.Read` | Payments |
| 系統錯誤 | SystemErrorLogs | `AuditLogs.Read` | AuditLogs |

Dashboard 顯示原則：

```text
□ 使用者沒有權限時，該卡片不顯示或顯示權限不足
□ 敏感金額只給 Analytics.Read 或管理角色
□ 卡片只做導向，不直接處理退款、出貨、調庫存
□ 所有數字都應標示資料期間，例如今日、近 7 日、本月
```

---

## 4. 待處理事項

待辦不應人工輸入，而應由規則產生。

| 待辦 | 產生規則 | 導向 |
|---|---|---|
| 待出貨訂單 | `PaymentStatus = Paid` 且 `ShippingStatus = Pending` | Shipments |
| 付款逾期訂單 | `PaymentStatus = Pending` 且超過付款期限 | Orders |
| 退款待審核 | `RefundStatus = Requested` | Refunds |
| 低庫存補貨 | `AvailableQty <= SafetyStockQty` | Inventory |
| 商品待補資料 | 商品 Published 前缺少 SKU / 圖片 / 價格 | Products |
| 促銷即將結束 | `EndAt` 在指定天數內 | Promotions |
| 登入失敗異常 | 短時間登入失敗過多 | AuditLogs |

---

## 5. 異常警示

警示要比待辦更嚴格，通常代表營運風險。

| 警示 | 條件 | 嚴重度 |
|---|---|---|
| 付款 Callback 重複 | 同 ProviderTransactionNo 多次回呼 | 高 |
| 庫存負數 | AvailableQty 或 OnHandQty 異常 | 高 |
| 退款超額嘗試 | RefundAmount 超過可退金額 | 高 |
| 訂單已付款但無保留庫存 | Paid 訂單缺 InventoryReservation | 高 |
| 出貨數量超過訂單數量 | ShipmentItems 數量異常 | 高 |
| 高權限角色異動 | RolePermissions 被修改 | 高 |
| 系統錯誤暴增 | SystemErrorLogs 短時間大量增加 | 中 |

Dashboard 遇到高嚴重度警示時，應該優先顯示在頁面上方。

---

## 6. 最近訂單與最近操作

### 最近訂單

欄位：

```text
□ 訂單編號
□ 會員
□ 金額
□ 付款狀態
□ 出貨狀態
□ 建立時間
□ 操作
```

### 最近後台操作

資料來源：

```text
AdminActionLogs
AuditLogs
```

欄位：

```text
□ 操作者
□ 模組
□ 動作
□ 目標資料
□ 操作時間
□ IP
```

---

## 7. Dashboard Service 設計

目前已有：

```text
IAdminPageService.GetDashboardAsync
AdminPageService.GetDashboardAsync
```

後續建議拆分正式查詢 Service：

```text
IAdminDashboardService
├─ GetMetricCardsAsync
├─ GetWorkItemsAsync
├─ GetSystemAlertsAsync
├─ GetRecentOrdersAsync
├─ GetRecentAuditEventsAsync
└─ GetQuickLinksAsync
```

資料查詢應走 Query Service 或 Repository，不要在 Controller 查 DbContext。

---

## 8. 權限感知 Dashboard

Dashboard 必須依權限組合顯示不同內容。

| 角色 | Dashboard 重點 |
|---|---|
| SystemAdmin | 系統警示、稽核、權限異動、所有營運指標 |
| OperationsManager | 今日營收、訂單、出貨、退款、促銷表現 |
| ProductManager | 商品待補資料、低庫存、SKU 狀態 |
| OrderStaff | 待付款、待出貨、顧客訊息 |
| FinanceStaff | 付款成功率、退款待審、對帳異常 |
| MarketingStaff | 促銷表現、優惠券使用、活動即將結束 |
| Auditor | 最近高風險操作、角色異動、系統紀錄 |

---

## 9. 每日營運 SOP

Dashboard 應支援下列每日流程：

```text
1. 登入後台
2. 查看高嚴重度警示
3. 查看待出貨與退款申請
4. 查看低庫存 SKU
5. 查看付款失敗或異常訂單
6. 查看今日營收與訂單趨勢
7. 查看最近後台操作是否異常
8. 進入對應模組處理問題
```

---

## 10. 驗收條件

```text
□ Dashboard 只顯示使用者有權限看的卡片
□ 金額與分析資料不外洩給無 Analytics.Read 的角色
□ Dashboard 不直接執行退款、出貨、調庫存等高風險操作
□ 待辦與警示可以導向正確模組
□ 低庫存、付款失敗、退款待審可被清楚看到
□ 最近後台操作能追蹤操作者、模組、動作與時間
□ 不再使用靜態假資料作為正式營運數據
```
