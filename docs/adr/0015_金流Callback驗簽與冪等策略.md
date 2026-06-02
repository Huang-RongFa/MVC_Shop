# ADR-0015｜金流 Callback 驗簽與冪等策略

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`06_RESTfulAPI前後台API與交易流程.md`、`07_OWASP與API資安檢查清單.md`

## 1. 背景與問題

金流平台可能會因網路問題重送 Callback。攻擊者也可能嘗試偽造付款成功通知。若系統沒有驗簽與冪等處理，可能發生偽造付款、重複付款紀錄、重複更新訂單狀態或重複退款。

## 2. 決策

所有金流 Callback、退款 Callback 與物流 Callback 必須具備驗簽、來源檢查、交易編號唯一性與冪等處理。

## 3. 理由

- Callback 不是一般前端請求，不能只靠登入 Cookie。
- 付款狀態是高價值資料，必須驗證來源與內容完整性。
- 重送 Callback 是正常情境，系統必須安全處理重複請求。

## 4. 實作要求

```text
□ Callback 不依賴使用者登入狀態
□ 使用金流提供的簽章或驗證機制
□ 驗證 MerchantId、TradeNo、Amount、Status、Signature
□ PaymentTransactions 保存外部交易編號
□ 外部交易編號需有唯一限制或冪等鍵
□ 同一筆成功 Callback 重送不得重複更新金額
□ Callback 原始摘要或必要欄位需保存供稽核
□ 驗簽失敗回拒絕並寫入安全紀錄
```

## 5. 驗收檢查

```text
□ 偽造 Callback 不會更新訂單
□ 同一筆 Callback 重送結果一致
□ 金額不符不會標記付款成功
□ 付款成功只進入 Paid，不自動出貨
□ 異常 Callback 可在日誌中追蹤
```
