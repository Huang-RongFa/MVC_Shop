# FreshMart 後台管理模式設計文件總覽

## 結論

本文件組建議放置於：

```text
docs/admin-design/
```

這一組文件不是單純補頁面清單，而是把後台管理拆成「權限與系統、商品與交易、行銷與洞察、營運總覽」四個可控管模組，並補上角色權限、資料流、API / Service / 資料表對應、稽核紀錄與驗收規則。

---

## 文件清單

| 順序 | 檔案 | 用途 |
|---|---|---|
| 00 | `00_admin_control_model_overview.md` | 定義後台總體管理模式、模組邊界與控制原則 |
| 01 | `01_permissions_and_system.md` | 權限與系統設計：角色、權限、登入、稽核、系統設定 |
| 02 | `02_products_and_transactions.md` | 商品與交易設計：商品、SKU、庫存、訂單、付款、出貨、退款 |
| 03 | `03_marketing_and_insights.md` | 行銷與洞察設計：優惠券、促銷、訊息通知、數據分析 |
| 04 | `04_operations_dashboard.md` | 營運總覽設計：Dashboard、待辦、警示、營運工作台 |
| 05 | `05_admin_api_service_data_mapping.md` | 後台 Route / Policy / API / Service / 資料表對應總表 |
| 06 | `06_admin_development_acceptance_checklist.md` | 開發順序、Codex 任務拆分與驗收清單 |

---

## 與目前專案的對應

目前專案已經具備下列基礎，可直接銜接本文件：

```text
MyMVC/Controllers/AdminController.cs
MyMVC/Views/Admin/*.cshtml
MyMVC/Views/Shared/_AdminLayout.cshtml
MyMVC/Application/Services/Pages/AdminPageService.cs
MyMVC/Domain/Entities/Accounts/*
MyMVC/Domain/Entities/Catalog/*
MyMVC/Domain/Entities/Inventory/*
MyMVC/Domain/Entities/Orders/*
MyMVC/Domain/Entities/Payments/*
MyMVC/Domain/Entities/Promotions/*
MyMVC/Domain/Entities/Logs/*
```

目前已有的後台選單分類：

```text
營運總覽
商品與交易
行銷與洞察
權限與系統
```

因此，本文件建議保留現有 MVC / Razor 後台頁面入口，再逐步補上正式的 Admin API、Service、Repository、DTO 與稽核流程。

---

## 後續建議

先從 `00_admin_control_model_overview.md` 與 `01_permissions_and_system.md` 開始，確認權限與模組邊界後，再進入商品、交易與行銷功能開發。
