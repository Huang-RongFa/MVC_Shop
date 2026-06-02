# ADR-0007｜分離前台 API 與後台 API 邊界

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`06_RESTfulAPI前後台API與交易流程.md`、`05_前台與後台Vue正式部署流程.md`

## 1. 背景與問題

前台顧客與後台管理人員可能操作相同資料表，例如 `Products`、`Orders`、`Payments`。但兩者的查詢目的、回傳欄位、授權規則與可執行動作完全不同。若共用 Controller 與 DTO，系統成長後容易造成越權、資料外洩與維護困難。

## 2. 決策

前台 API 與後台 API 必須分離 Route、Controller、DTO 與授權邏輯。

```text
/api/storefront/*：顧客購物與自助查詢
/api/admin/*：後台營運管理
```

## 3. 理由

- 前台 API 著重公開商品、會員自己的購物車與訂單。
- 後台 API 著重商品維護、庫存、付款、出貨、退款與稽核。
- DTO 分離可避免後台欄位外洩給前台。
- 授權分離可降低 Broken Object Level Authorization 風險。

## 4. 實作要求

```text
□ Controller 分為 Storefront 與 Admin
□ DTO 分為 Storefront 與 Admin
□ Storefront API 必須檢查資料擁有者
□ Admin API 必須檢查 Role / Permission
□ 不可只靠前端隱藏按鈕控權
□ Service 層需再次檢查重要操作權限
□ Swagger / OpenAPI 應清楚分組前台與後台 API
```

## 5. 影響與取捨

| 面向 | 影響 |
|---|---|
| 優點 | 權限、DTO、查詢語意清楚 |
| 成本 | Controller 與 DTO 數量增加 |
| 風險 | 若 Service 共用時未檢查情境，仍可能發生越權 |

## 6. 驗收檢查

```text
□ 前台無法呼叫後台 API
□ 前台會員只能查自己的訂單
□ 後台操作需對應權限
□ API Response 不回傳不必要欄位
□ 401 / 403 / 404 語意一致
```
