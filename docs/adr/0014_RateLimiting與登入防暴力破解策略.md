# ADR-0014｜Rate Limiting 與登入防暴力破解策略

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`03_Program與Middleware安全管線.md`、`04_登入驗證Cookie權限與CSRF安全.md`、`07_OWASP與API資安檢查清單.md`

## 1. 背景與問題

正式啟用後，公開 API 可能遭遇暴力登入、惡意註冊、商品搜尋濫用、報表查詢拖垮系統、金流 Callback 重送或簡單 DoS 攻擊。若沒有速率限制與登入防護，網站容易被少量惡意流量拖慢或癱瘓。

## 2. 決策

正式環境必須導入 Rate Limiting，並對登入、註冊、忘記密碼、搜尋、報表、Callback 等高風險 API 採取不同限制策略。

建議分級：

```text
Auth API：嚴格限制，依 IP + Account 限制
Public Product API：中等限制，避免搜尋濫用
Admin API：登入後依使用者與角色限制
Payment Callback：允許重送但需驗簽與冪等
Report API：限制併發與查詢區間
```

## 3. 理由

- 降低暴力破解與濫用風險。
- 保護資料庫避免被大量查詢拖垮。
- 讓正常使用者有公平資源。
- 與登入失敗紀錄、帳號鎖定、監控告警搭配使用。

## 4. 實作要求

```text
□ Program.cs 註冊 AddRateLimiter 與 UseRateLimiter
□ Login 失敗寫入 UserLoginLogs
□ 多次失敗可暫時鎖定帳號或增加等待時間
□ 忘記密碼與註冊 API 需限制頻率
□ 報表 API 限制日期範圍與併發
□ Rate Limit 被拒絕時回 429
□ 記錄異常高頻請求供監控告警
```

## 5. 驗收檢查

```text
□ 短時間大量登入會被限制
□ 被限制時回 429，而不是拖垮資料庫
□ 正常使用者不受過度影響
□ UserLoginLogs 可查詢失敗次數與來源
□ 管理員可追蹤異常 IP 或帳號
```
