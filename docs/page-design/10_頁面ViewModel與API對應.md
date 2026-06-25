# 10｜頁面 ViewModel 與 API 對應

## 1. 為什麼需要 ViewModel

Razor View 不應直接使用 Entity，也不應直接查 DbContext。每個頁面應使用 ViewModel 承接畫面需要的資料。

```text
Entity：資料庫模型
DTO：API 輸入輸出模型
ViewModel：Razor View 顯示模型
```

---

## 2. 前台 ViewModel

### HomeViewModel

```text
FeaturedProducts
Categories
PromotionBanners
IsAuthenticated
CartItemCount
```

### ProductListViewModel

```text
Products
Categories
SelectedCategoryId
Keyword
Page
PageSize
TotalCount
```

### ProductDetailViewModel

```text
ProductId
ProductName
Description
Images
Skus
PriceRange
CanAddToCart
IsAuthenticated
```

### CartViewModel

```text
CartItems
Subtotal
DiscountTotal
EstimatedTotal
CanCheckout
```

### CheckoutViewModel

```text
CartItems
ShippingInfo
PaymentMethods
CouponCode
OrderSummary
```

---

## 3. 後台 ViewModel

### AdminDashboardViewModel

```text
TodayRevenue
TodayOrderCount
PendingShipmentCount
PendingRefundCount
LowStockProductCount
PaymentFailedCount
RecentOrders
RecentAdminActions
SystemAlerts
```

### AdminProductListViewModel

```text
Products
Categories
Statuses
Keyword
Page
PageSize
TotalCount
CanCreate
CanEdit
CanPublish
```

### AdminOrderListViewModel

```text
Orders
OrderStatuses
PaymentStatuses
ShipmentStatuses
DateFrom
DateTo
Page
PageSize
TotalCount
CanUpdateStatus
CanCreateShipment
CanRefund
```

### AdminAnalyticsViewModel

```text
RevenueTrend
OrderTrend
TopSellingProducts
CategorySales
PaymentSuccessRate
RefundRate
MemberGrowth
ConversionRate
```

---

## 4. Controller 對應

| View | Controller Action | Service |
|---|---|---|
| `Home/Index` | `HomeController.Index` | `StorefrontHomeService` |
| `Products/Index` | `ProductsController.Index` | `ProductService` |
| `Products/Details` | `ProductsController.Details` | `ProductService` |
| `Cart/Index` | `CartController.Index` | `CartService` |
| `Checkout/Index` | `CheckoutController.Index` | `CheckoutService` |
| `Orders/Index` | `OrdersController.Index` | `OrderService` |
| `Admin/Index` | `AdminController.Index` | `AdminDashboardService` |
| `Admin/Analytics` | `AdminController.Analytics` | `AdminAnalyticsService` |
| `Admin/Products` | `AdminProductsController.Index` | `ProductService` |
| `Admin/Orders` | `AdminOrdersController.Index` | `OrderService` |

---

## 5. MVC View 與 API 的關係

目前若使用 MVC Razor：

```text
瀏覽器 → Controller → Service → ViewModel → cshtml
```

未來若改 Vue：

```text
Vue → API Controller → Service → DTO → JSON
```

Service 可以共用，但 ViewModel 與 API DTO 應分開，不要讓 Razor View 綁死 API Response 格式。

---

## 6. 本章總結

每個頁面都應該先定義 ViewModel，再切 View。不要讓 `.cshtml` 直接使用 Entity 或硬寫假資料；也不要讓 Controller 自己組合複雜商業規則。
