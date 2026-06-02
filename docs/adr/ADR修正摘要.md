# ADR 修正摘要｜正式部署校正版

## 1. 本次修正重點

原始 0001～0011 ADR 已能描述核心架構，但內容偏短，較像決策備註。這次已改成正式 ADR 格式，補上背景、決策、理由、實作要求、影響與驗收檢查。

## 2. 重要新增內容

```text
□ ADR-0008 改為 Cookie + CSRF，不只寫 Cookie
□ 新增 ADR-0012 環境分離
□ 新增 ADR-0013 CORS 與可信任網域
□ 新增 ADR-0014 Rate Limiting 與登入防暴力破解
□ 新增 ADR-0015 金流 Callback 驗簽與冪等
□ 新增 ADR-0016 Swagger / OpenAPI 正式環境管控
□ 新增 ADR-0017 資料庫備份、還原與遷移
□ 新增 ADR-0018 Health Check、Logging、Monitoring 與告警
□ 新增 ADR-0019 前台 SEO 與部署模式
□ 新增 ADR-0020 圖片上傳與檔案安全
```

## 3. 不應再出現的錯誤方向

```text
錯誤：正式登入主要使用 localStorage JWT
正確：正式瀏覽器登入以 HttpOnly Secure Cookie 為主，並搭配 CSRF 防護

錯誤：付款成功後直接出貨並扣庫存
正確：付款成功只進入 Paid，出貨確認時才扣實際庫存

錯誤：前台與後台共用 Controller 與 DTO
正確：前台 API 與後台 API 分離 Route、Controller、DTO 與授權

錯誤：正式環境使用 AllowAnyOrigin
正確：正式環境 CORS 只允許指定前台與後台網域

錯誤：部署後再考慮監控與備份
正確：正式啟用前就要建立 Health Check、Logging、告警、備份與還原流程
```
