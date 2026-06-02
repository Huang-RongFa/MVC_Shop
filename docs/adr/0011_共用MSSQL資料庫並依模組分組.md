# ADR-0011｜共用 MSSQL 資料庫並依模組分組

- 狀態：已採納
- 日期：2026-06-02
- 關聯文件：`01_資料庫與EFCore正式對應.md`、`08_完整系統開發部署流程清單.md`

## 1. 背景與問題

本專案的訂單、付款、庫存、退款、優惠與稽核流程高度相關。若初期拆成多個資料庫，將增加分散式交易、資料同步、查詢整合與備份還原成本。

## 2. 決策

所有電商模組初期共用同一套 MSSQL 資料庫，並透過資料表命名、文件分組、Entity 分組與 Service 邊界表達模組化。

```text
Accounts
Catalog
Inventory
Orders
Payments
Promotions
Logs
```

## 3. 理由

- 符合模組化單體架構。
- 跨表交易較容易保持一致。
- 開發、部署、備份與還原成本較低。
- 對一邊學習一邊正式部署的階段較穩定。

## 4. 實作要求

```text
□ 建表 SQL 與 EF Core Entity 必須一致
□ 重要欄位需有 PK、FK、Index、Unique、Check Constraint
□ 跨表寫入流程必須使用 Transaction
□ 讀取大量資料需使用分頁與索引
□ 正式環境需定期備份與測試還原
□ Migration 或 SQL Script 需有版本管理策略
□ 不得讓前端直接連線資料庫
```

## 5. 影響與取捨

| 面向 | 影響 |
|---|---|
| 優點 | 交易一致性與部署維運較簡單 |
| 成本 | 單一資料庫需要良好索引、備份與權限管理 |
| 風險 | 若資料庫無備份或索引設計不良，正式上線後風險高 |

## 6. 驗收檢查

```text
□ 建立訂單可同時寫入 Orders / OrderItems / Reservations
□ 付款成功可寫入 Payments / Transactions / OrderStatus
□ 出貨扣庫可寫入 Shipments / InventoryTransactions
□ 資料庫有備份與還原演練
□ Connection String 不進 Git
```
