# ADR-0016｜Swagger 與 OpenAPI 正式環境管控策略

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`03_Program與Middleware安全管線.md`、`08_完整系統開發部署流程清單.md`、`09_常用指令與安全檔案模板.md`

## 1. 背景與問題

Swagger 對開發與測試非常有用，但正式環境若公開全部 API 文件，可能讓攻擊者更容易理解後台路由、參數格式與高風險操作。

## 2. 決策

Development 環境可公開 Swagger；Staging 可限制網段或加授權；Production 預設不公開 Swagger，若必須公開則需加上身份驗證、IP 限制或內部網路保護。

## 3. 理由

- 減少正式環境攻擊面。
- 保留開發與測試效率。
- API 文件應服務團隊，而不是無限制公開後台操作資訊。

## 4. 實作要求

```text
□ Program.cs 依環境判斷 Swagger 啟用方式
□ Production 預設關閉 Swagger UI
□ 若 Production 需要 Swagger，必須加授權或 IP 限制
□ OpenAPI 分組 Storefront / Admin
□ 不在 Swagger 範例中放正式金鑰、Cookie 或敏感資料
□ 不回傳內部 Stack Trace 到 Swagger 測試結果
```

## 5. 驗收檢查

```text
□ Development 可使用 Swagger 測試 API
□ Production 無法匿名瀏覽後台 API 文件
□ Swagger 範例不包含機密
□ API 文件分清前台與後台
```
