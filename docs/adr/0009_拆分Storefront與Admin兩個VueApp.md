# ADR-0009｜拆分 Storefront 與 Admin 兩個 Vue App

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`05_前台與後台Vue正式部署流程.md`、`00_總覽與正式上線架構.md`

## 1. 背景與問題

前台顧客頁面與後台管理頁面的目標不同。前台重視商品瀏覽、購物體驗、SEO、效能與轉換率；後台重視權限、資料維護、稽核、操作效率與安全控管。若放在同一個 Vue App，會增加打包體積、路由複雜度與高權限功能外洩風險。

## 2. 決策

前端拆成兩個 Vue App：

```text
frontend/storefront：前台顧客購物 App
frontend/admin：後台管理 App
```

建議部署網域：

```text
https://www.example.com
https://admin.example.com
https://api.example.com
```

## 3. 理由

- 前台與後台的使用者、UI、權限、路由與 API 邊界不同。
- 可分別設定部署、快取、SEO、CSP 與安全策略。
- 後台資源不會被打包到公開前台頁面。
- 與 `/api/storefront/*`、`/api/admin/*` 的 API 分界一致。

## 4. 實作要求

```text
□ Storefront 與 Admin 使用不同 Router
□ Storefront 與 Admin 使用不同 API Module
□ Admin App 必須有登入守衛與權限守衛
□ 前端不可保存後端機密
□ 高權限 Token 不放 localStorage
□ API Client 統一處理 401 / 403 / CSRF / Loading / Error
□ Admin App 不應被搜尋引擎索引
□ Storefront 若要 SEO，需另評估 SSR / SSG 或前端 SEO 策略
```

## 5. 影響與取捨

| 面向 | 影響 |
|---|---|
| 優點 | 前後台責任清楚，部署與安全策略可分開 |
| 成本 | 需維護兩個 Vue 專案與共用元件策略 |
| 風險 | 若兩個 App 共用不當設定，可能造成 Cookie / CORS 錯誤 |

## 6. 驗收檢查

```text
□ 前台頁面不包含後台路由
□ 後台頁面需登入才能進入
□ 後台按鈕隱藏之外，後端仍檢查權限
□ 前後台可分別部署與 Build
□ 兩個 App 呼叫正確 API 邊界
```
