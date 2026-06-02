# 12 ERD 文字版關聯整理

## 1. 帳戶與權限模組

```text
Users 1 ── N UserRoles
Roles 1 ── N UserRoles
Roles 1 ── N RolePermissions
Permissions 1 ── N RolePermissions
Users 1 ── N UserLoginLogs
```

說明：

- 使用者與角色為多對多關係。
- 角色與權限為多對多關係。
- 使用者登入會產生多筆登入紀錄。

---

## 2. 商品模組

```text
ProductCategories 1 ── N Products
ProductCategories 1 ── N ProductCategories
Products 1 ── N ProductSkus
Products 1 ── N ProductImages
ProductSkus 1 ── N ProductImages
ProductSkus 1 ── N ProductAttributeValues
ProductAttributes 1 ── N ProductAttributeValues
```

說明：

- 商品分類支援父子分類。
- 一個商品可以有多個 SKU。
- 一個商品可以有多張圖片。
- 一個 SKU 可以有多個屬性值。

---

## 3. 庫存模組

```text
Warehouses 1 ── N InventoryStocks
ProductSkus 1 ── N InventoryStocks
Warehouses 1 ── N InventoryTransactions
ProductSkus 1 ── N InventoryTransactions
Orders 1 ── N InventoryReservations
OrderItems 1 ── N InventoryReservations
ProductSkus 1 ── N InventoryReservations
```

說明：

- 一個 SKU 可以存在多個倉庫。
- 每次庫存變化都應建立異動紀錄。
- 訂單成立時可建立保留庫存。

---

## 4. 購物車與訂單模組

```text
Users 1 ── N ShoppingCarts
ShoppingCarts 1 ── N ShoppingCartItems
ProductSkus 1 ── N ShoppingCartItems
Users 1 ── N Orders
Orders 1 ── N OrderItems
Products 1 ── N OrderItems
ProductSkus 1 ── N OrderItems
Orders 1 ── N OrderStatusHistories
```

說明：

- 一個使用者可以有多筆購物車紀錄。
- 購物車明細會對應 SKU。
- 訂單主檔保存總金額與收件資訊。
- 訂單明細保存商品 Snapshot。

---

## 5. 付款、物流與退款模組

```text
Orders 1 ── N Payments
Payments 1 ── N PaymentTransactions
Orders 1 ── N Shipments
Shipments 1 ── N ShipmentItems
OrderItems 1 ── N ShipmentItems
Orders 1 ── N Refunds
Payments 1 ── N Refunds
Refunds 1 ── N RefundItems
OrderItems 1 ── N RefundItems
```

說明：

- 一筆訂單可有多次付款嘗試。
- 一筆付款可有多筆金流交易紀錄。
- 一筆訂單可分批出貨。
- 一筆訂單可部分退款。

---

## 6. 優惠模組

```text
Coupons 1 ── N CouponUsages
Users 1 ── N CouponUsages
Orders 1 ── N CouponUsages
Promotions 1 ── N PromotionProducts
Products 1 ── N PromotionProducts
ProductSkus 1 ── N PromotionProducts
Orders 1 ── N OrderDiscounts
Coupons 1 ── N OrderDiscounts
Promotions 1 ── N OrderDiscounts
```

說明：

- 優惠券使用後需保存使用紀錄。
- 促銷活動可指定商品或 SKU。
- 訂單需保存實際折扣紀錄。

---

## 7. 稽核模組

```text
Users 1 ── N AuditLogs
Users 1 ── N AdminActionLogs
Users 1 ── N SystemErrorLogs
```

說明：

- 使用者操作資料時建立 AuditLogs。
- 後台功能操作建立 AdminActionLogs。
- 系統錯誤建立 SystemErrorLogs。

---

## 8. 簡化整體 ERD

```text
Users
 ├─ UserRoles ─ Roles ─ RolePermissions ─ Permissions
 ├─ ShoppingCarts ─ ShoppingCartItems ─ ProductSkus
 ├─ Orders ─ OrderItems ─ ProductSkus ─ Products ─ ProductCategories
 ├─ CouponUsages ─ Coupons
 └─ AdminActionLogs / AuditLogs / UserLoginLogs

Products
 ├─ ProductSkus
 │   ├─ InventoryStocks ─ Warehouses
 │   ├─ InventoryTransactions
 │   ├─ ProductAttributeValues ─ ProductAttributes
 │   └─ OrderItems
 └─ ProductImages

Orders
 ├─ OrderItems
 ├─ Payments ─ PaymentTransactions
 ├─ Shipments ─ ShipmentItems
 ├─ Refunds ─ RefundItems
 ├─ OrderDiscounts
 ├─ OrderStatusHistories
 └─ InventoryReservations
```

---

## 9. 關聯設計重點

1. 所有交易主檔都使用系統內部 ID 作為 FK。
2. 所有對外顯示的編號使用 TK，例如 OrderNo、PaymentNo。
3. 訂單明細保存商品 Snapshot，避免歷史資料被商品主檔修改影響。
4. 庫存現況與庫存異動分離。
5. 優惠規則與實際折扣紀錄分離。
6. 稽核紀錄獨立保存，不依賴單一模組。
