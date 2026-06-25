# 09｜前端正式上線、SEO、可用性與瀏覽器規範

> 版本：2026-06-02  
> 文件目的：確保 Storefront 與 Admin 兩個 Vue App 在正式環境中具備正確建置、SEO、可用性、安全與錯誤處理。

---

## 1. 本章目的

兩個 Vue App 正式上線時，不只是畫面能跑，還要確認前端建置、快取、錯誤追蹤、瀏覽器相容、SEO 與可用性。

```text
storefront：前台顧客使用
admin：後台管理員與員工使用
```

---

## 2. Build 規範

```text
□ 使用 Production Build
□ Storefront API Base URL 使用正式環境設定
□ Admin API Base URL 使用正式環境設定
□ Source Map 是否公開需評估
□ JS / CSS 檔名帶 hash
□ 移除 console.log 或避免輸出敏感資料
□ 前端環境變數不包含後端機密
□ Build Artifact 有版本號與 Git Commit
```

---

## 3. SEO 規劃

如果前台商品頁需要被搜尋引擎收錄，純 SPA 可能需要額外處理。

可考慮：

```text
□ 商品頁 Meta Title / Description
□ Open Graph
□ Sitemap
□ robots.txt
□ Canonical URL
□ 結構化資料 Structured Data
□ SSR / SSG 或預渲染
```

後台管理頁通常不需要 SEO，應避免被搜尋引擎收錄。

```text
□ admin robots.txt 禁止索引
□ admin 頁面加上 noindex
□ admin 不出現在 sitemap
□ admin 不被公開導覽連結曝光
```

---

## 4. 可用性與瀏覽器

```text
□ 桌機瀏覽器測試
□ 手機瀏覽器測試
□ 表單錯誤提示清楚
□ 按鈕 Loading 狀態
□ API 失敗時有提示
□ 登入狀態過期導回登入
□ 網路中斷時提示使用者
□ 付款流程中避免重複點擊
□ 表單送出後避免重複提交
```

---

## 5. 前端安全

```text
□ 不保存 Cookie / Token 簽章金鑰
□ 不保存資料庫密碼
□ 不在 localStorage 保存敏感個資
□ 不在 localStorage 長期保存高權限 Token
□ 避免 v-html 顯示未清理內容
□ 權限按鈕隱藏只是 UX，後端仍要檢查
□ 使用 withCredentials 時確認 CORS 與 CSRF 設定
□ 不把錯誤中的 Stack Trace 顯示在畫面
```

---

## 6. 後台管理頁

後台建議：

```text
□ admin 網域與前台分開
□ admin App 與 storefront App 分開建置或分開部署
□ 後台頁面不被搜尋引擎索引
□ 高風險操作二次確認
□ 退款、庫存調整、權限異動需留紀錄
□ 登入過期自動導回登入頁
□ 無權限操作顯示 403，不只隱藏按鈕
```

---

## 7. 前端錯誤追蹤

建議紀錄：

```text
□ 前端版本號
□ 發生頁面
□ API Path
□ Status Code
□ TraceId / RequestId
□ 瀏覽器與裝置資訊
□ 錯誤訊息
```

避免紀錄：

```text
□ 密碼
□ 完整 Cookie
□ 完整 Token
□ 信用卡資料
□ 完整個資
```

---

## 8. 前端快取策略

```text
□ JS / CSS 使用檔名 hash + 長快取
□ index.html 使用短快取或不快取
□ 圖片可 CDN 快取
□ API 回應依資料敏感度設定 Cache-Control
□ Admin 頁面與個人資料頁不公開快取
```

---

## 9. 本章總結

前端正式上線要同時看建置、快取、SEO、瀏覽器、錯誤體驗與安全。前台重視使用者體驗與搜尋曝光，後台重視權限、稽核與操作安全。

---

## 參考基準

- OWASP Top 10: https://owasp.org/www-project-top-ten/
- OWASP API Security Top 10 2023: https://owasp.org/API-Security/editions/2023/en/0x11-t10/
- OWASP CSRF Prevention Cheat Sheet: https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html
- Microsoft ASP.NET Core CORS: https://learn.microsoft.com/aspnet/core/security/cors
- Microsoft ASP.NET Core Anti-forgery: https://learn.microsoft.com/aspnet/core/security/anti-request-forgery
- Microsoft ASP.NET Core Rate Limiting: https://learn.microsoft.com/aspnet/core/performance/rate-limit
- Microsoft ASP.NET Core Health Checks: https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks
