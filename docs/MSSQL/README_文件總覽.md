# MSSQL 電商後台資料庫完整設計文件

本文件提供一套適合全端開發專案使用的 MSSQL 電商後台資料庫設計，適用於 ASP.NET Core MVC / Web API / Vue / React / Angular 等前後端分離架構。資料庫設計涵蓋：使用者帳戶、角色權限、商品品項、SKU、庫存、購物車、訂單、付款、物流、優惠券、退款、後台操作紀錄與資料稽核。

---

## 文件目錄

| 檔案 | 說明 |
|---|---|
| `01_資料庫系統範圍與設計原則.md` | 系統範圍、資料庫命名原則、資料分層概念 |
| `02_帳戶角色與權限資料表設計.md` | 使用者、角色、權限、登入紀錄設計 |
| `03_商品分類SKU與規格資料表設計.md` | 商品、分類、SKU、圖片、規格設計 |
| `04_庫存倉庫與庫存異動資料表設計.md` | 庫存、倉庫、庫存異動、保留庫存設計 |
| `05_購物車訂單與訂單明細資料表設計.md` | 購物車、訂單主檔、訂單明細設計 |
| `06_付款物流與退款資料表設計.md` | 付款、物流、退款設計 |
| `07_優惠券促銷與折扣資料表設計.md` | 優惠券、促銷活動、折扣紀錄設計 |
| `08_後台操作紀錄稽核與系統紀錄設計.md` | 後台操作紀錄、登入紀錄、資料異動追蹤 |
| `09_主鍵外鍵唯一鍵檢查限制索引與追蹤鍵設計說明.md` | PK、FK、UK、CK、Index、TK 設計說明 |
| `10_MSSQL電商後台資料庫完整實作計畫.md` | 完整實作計畫與工作細則 |
| `11_mssql_create_table_script.sql` | MSSQL 建表 SQL 範例 |
| `12_ERD文字版資料表關聯整理.md` | ERD 文字版資料表關聯整理 |
| `13_後端API與資料表對應規劃.md` | 後端 API 與資料表對應規劃 |
| `14_安全性交易一致性與資料完整性建議.md` | 安全性、交易一致性、資料完整性建議 |
| `15_MSSQL電商後台資料庫部署檢查清單.md` | 資料庫建置、測試、部署檢查清單 |

---

## 資料庫設計目標

本資料庫設計目標不是只有建立幾張商品與訂單表，而是建立一套可擴充的電商後台資料結構，能支援後續系統開發、API 串接、權限控管、報表查詢與資料稽核。

主要目標如下：

1. **支援商品管理**  
   可管理商品分類、商品主檔、SKU、商品圖片、規格屬性與上下架狀態。

2. **支援帳號與權限控管**  
   可管理前台會員與後台管理員，並透過角色與權限控制不同功能的存取範圍。

3. **支援訂單流程**  
   從購物車、成立訂單、付款、出貨、取消、退款到完成訂單，皆有資料表可對應。

4. **支援庫存控管**  
   可記錄商品 SKU 庫存、倉庫、庫存異動、保留庫存與扣庫存流程。

5. **支援優惠活動**  
   可建立優惠券、促銷活動、折扣條件、使用限制與訂單折扣紀錄。

6. **支援後台稽核**  
   可記錄後台人員登入、操作、資料新增、修改、刪除與異常行為。

7. **支援全端開發擴充**  
   資料表可對應 ASP.NET Core Web API、Entity Framework Core、Dapper、Vue 前端頁面與後台管理功能。

---

## 建議資料庫命名

```sql
CREATE DATABASE EcommerceBackendDB;
```

建議資料表採用清楚且具語意的命名，例如：

```text
Users
Roles
Permissions
Products
ProductCategories
ProductSkus
InventoryStocks
Orders
OrderItems
Payments
Shipments
Coupons
AuditLogs
```

---

## 整體資料模組

```text
帳戶權限模組
├─ Users
├─ Roles
├─ UserRoles
├─ Permissions
├─ RolePermissions
└─ UserLoginLogs

商品模組
├─ ProductCategories
├─ Products
├─ ProductSkus
├─ ProductImages
├─ ProductAttributes
└─ ProductAttributeValues

庫存模組
├─ Warehouses
├─ InventoryStocks
├─ InventoryTransactions
└─ InventoryReservations

購物車與訂單模組
├─ ShoppingCarts
├─ ShoppingCartItems
├─ Orders
├─ OrderItems
└─ OrderStatusHistories

付款物流退款模組
├─ Payments
├─ PaymentTransactions
├─ Shipments
├─ ShipmentItems
├─ Refunds
└─ RefundItems

優惠模組
├─ Coupons
├─ CouponUsages
├─ Promotions
├─ PromotionProducts
└─ OrderDiscounts

稽核模組
├─ AuditLogs
├─ AdminActionLogs
└─ SystemErrorLogs
```

---

## 建議實作順序

1. 建立資料庫與基礎 Schema。
2. 建立帳號、角色、權限相關資料表。
3. 建立商品分類、商品主檔、SKU 與圖片資料表。
4. 建立倉庫、庫存與庫存異動資料表。
5. 建立購物車、訂單與訂單明細資料表。
6. 建立付款、物流與退款資料表。
7. 建立優惠券、促銷與折扣資料表。
8. 建立稽核紀錄與登入紀錄資料表。
9. 建立 Index、Constraint、View、Stored Procedure。
10. 匯入測試資料並進行 CRUD 測試。
11. 串接後端 API。
12. 串接前端管理頁面。
13. 進行壓力測試、交易測試與權限測試。
14. 部署正式資料庫。

---

## 關於 PK、FK、UK、CK、TK 的簡要說明

| 縮寫 | 名稱 | 用途 |
|---|---|---|
| PK | Primary Key | 主鍵，用來唯一識別每一筆資料 |
| FK | Foreign Key | 外鍵，用來建立資料表之間的關聯 |
| UK | Unique Key | 唯一鍵，用來限制欄位不可重複 |
| CK | Check Constraint | 檢查限制，用來限制欄位值的合理範圍 |
| IDX | Index | 索引，用來提升查詢速度 |
| TK | Tracking Key / Transaction Key | 追蹤鍵或交易鍵，常用於訂單編號、付款編號、物流追蹤碼、庫存異動單號 |

> 注意：TK 不是 SQL Server 標準約束名稱，而是系統設計中常用的「可追蹤業務編號」。例如 OrderNo、PaymentNo、ShipmentNo、InventoryTransactionNo。

---

## 適合的專案架構

此資料庫可搭配以下架構：

```text
Frontend
├─ Vue / React / Angular
├─ 商品管理頁面
├─ 訂單管理頁面
├─ 會員管理頁面
├─ 優惠券管理頁面
└─ 報表儀表板

Backend
├─ ASP.NET Core MVC / Web API
├─ Controllers
├─ Services
├─ Repositories
├─ DTOs
├─ EF Core / Dapper
└─ Authentication / Authorization

Database
├─ MSSQL
├─ Tables
├─ Views
├─ Stored Procedures
├─ Indexes
└─ Transaction Control
```
