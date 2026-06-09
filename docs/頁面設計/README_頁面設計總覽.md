# 頁面設計總覽

## 1. 文件目的

本資料夾用來控管目前 ASP.NET Core MVC `Views` 版面設計需求，讓前台購物頁面、後台管理頁面、登入頁面、CSS、JavaScript 與正式上線資安規範有一致的製作順序。

目前已有 DayNight Admin HTML / CSS / JS 版型檔案，可作為 Razor `.cshtml` 的切版來源，但不能直接原封不動放進正式系統。必須依照前台與後台邊界重新拆分 Layout、View、ViewModel、權限與資料來源。

---

## 2. 本階段採用方式

本專案長期架構仍維持：

```text
ASP.NET Core Web API 模組化單體
+
共用 MSSQL Database
+
Storefront App
+
Admin App
```

但目前正在使用 ASP.NET Core MVC `Views` 製作頁面，因此本階段採用：

```text
ASP.NET Core MVC Razor Views
+
Shared Layout
+
Storefront Views
+
Admin Views
+
Controller / Service / DTO / ViewModel
```

後續若改成 Vue App，這些文件仍可作為頁面資訊架構、元件切分、路由與 API 串接依據。

---

## 3. 頁面設計文件目錄

| 文件 | 用途 |
|---|---|
| `00_版型來源與整合原則.md` | 說明 DayNight 版型如何整合到 MVC Views |
| `01_前台與後台頁面邊界.md` | 劃分前台、後台與登入頁面 |
| `02_Views資料夾規劃.md` | 規劃 Razor Views 目錄與 Layout |
| `03_前台頁面設計清單.md` | 首頁、商品、購物車、結帳、會員訂單 |
| `04_後台頁面設計清單.md` | Dashboard、商品、庫存、訂單、付款、出貨、退款、優惠、稽核 |
| `05_登入註冊忘記密碼頁面設計.md` | 前台會員與後台管理員登入頁面 |
| `06_版型檔案對應與資源放置.md` | HTML / CSS / JS 對應到 `wwwroot` 與 `Views` |
| `07_切版轉換成cshtml步驟.md` | 從 HTML 改成 Razor View 的實作步驟 |
| `08_前台匿名與登入後流程.md` | 尚未登入可看商品、登入後可購買 |
| `09_後台權限與選單顯示規則.md` | 後台依 Role / Permission 顯示選單 |
| `10_頁面ViewModel與API對應.md` | 頁面資料來源、ViewModel 與 API 對應 |
| `11_CSS與JavaScript整合規範.md` | 版型 CSS / JS 整合、資安與可維護規範 |
| `12_頁面設計開發步驟總表.md` | 分階段完成頁面設計 |
| `13_頁面設計驗收清單.md` | 功能、權限、RWD、資安、正式上線驗收 |

---

## 4. 頁面設計核心原則

```text
前台：給顧客看商品、購物、結帳，不顯示後台數據分析。
後台：給管理員與員工管理商品、庫存、訂單、付款、出貨、退款、優惠與稽核。
登入：前台會員登入與後台管理登入應分開判斷身分與權限。
版型：HTML / CSS / JS 只作為視覺模板，不可直接當作正式商業邏輯。
資料：頁面資料必須來自 Controller / Service / ViewModel，不可長期使用假資料。
權限：前端隱藏選單只是 UX，後端 Controller / Service 仍必須檢查權限。
```
