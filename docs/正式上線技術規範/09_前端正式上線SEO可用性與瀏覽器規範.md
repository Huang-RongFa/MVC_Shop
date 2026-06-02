# 09｜前端正式上線、SEO、可用性與瀏覽器規範

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
□ SSR / SSG 或預渲染
```

後台管理頁通常不需要 SEO，應避免被搜尋引擎收錄。

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
```

---

## 7. 本章總結

前端正式上線要同時看建置、快取、SEO、瀏覽器、錯誤體驗與安全。前台重視使用者體驗與搜尋曝光，後台重視權限、稽核與操作安全。
