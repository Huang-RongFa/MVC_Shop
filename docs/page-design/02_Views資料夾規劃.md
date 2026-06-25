# 02｜Views 資料夾規劃

## 1. 目前狀態

目前專案已有 MVC `Views` 結構：

```text
Views/
├─ Admin/
│  ├─ Index.cshtml
│  └─ Login.cshtml
├─ Home/
│  ├─ Index.cshtml
│  └─ Privacy.cshtml
├─ Shared/
│  ├─ _Layout.cshtml
│  ├─ _Layout.cshtml.css
│  ├─ _ValidationScriptsPartial.cshtml
│  └─ Error.cshtml
├─ _ViewImports.cshtml
└─ _ViewStart.cshtml
```

這是可以開始切版的基礎，但若要支援完整電商，建議進一步拆分前台、後台與帳號頁面。

---

## 2. 建議 Views 結構

```text
Views/
├─ Home/
│  ├─ Index.cshtml                  前台首頁
│  ├─ About.cshtml                  關於我們
│  └─ Privacy.cshtml                隱私權政策
│
├─ Products/
│  ├─ Index.cshtml                  商品列表
│  ├─ Details.cshtml                商品詳細
│  └─ Category.cshtml               分類商品
│
├─ Cart/
│  └─ Index.cshtml                  購物車
│
├─ Checkout/
│  ├─ Index.cshtml                  結帳頁
│  └─ Complete.cshtml               訂單完成
│
├─ Orders/
│  ├─ Index.cshtml                  我的訂單
│  └─ Details.cshtml                我的訂單詳細
│
├─ Account/
│  ├─ Login.cshtml                  前台會員登入
│  ├─ Register.cshtml               前台會員註冊
│  ├─ ForgotPassword.cshtml         忘記密碼
│  └─ Profile.cshtml                會員資料
│
├─ Admin/
│  ├─ Login.cshtml                  後台登入
│  ├─ Index.cshtml                  後台 Dashboard
│  ├─ Analytics.cshtml              後台數據分析
│  ├─ Users.cshtml                  使用者管理
│  ├─ Roles.cshtml                  角色權限管理
│  ├─ Products.cshtml               商品管理
│  ├─ ProductEdit.cshtml            商品新增修改
│  ├─ ProductSkus.cshtml            SKU 管理
│  ├─ Inventory.cshtml              庫存查詢與調整
│  ├─ Orders.cshtml                 訂單管理
│  ├─ OrderDetails.cshtml           訂單詳細
│  ├─ Payments.cshtml               付款管理
│  ├─ Shipments.cshtml              出貨管理
│  ├─ Refunds.cshtml                退款管理
│  ├─ Coupons.cshtml                優惠券管理
│  ├─ Promotions.cshtml             促銷管理
│  ├─ Inbox.cshtml                  訊息 / 客服 / 通知
│  ├─ AuditLogs.cshtml              稽核紀錄
│  └─ Settings.cshtml               後台設定
│
└─ Shared/
   ├─ _StorefrontLayout.cshtml
   ├─ _AdminLayout.cshtml
   ├─ _AuthLayout.cshtml
   ├─ _StorefrontHeader.cshtml
   ├─ _AdminHeader.cshtml
   ├─ _MobileMenu.cshtml
   ├─ _ThemeToggle.cshtml
   ├─ _ValidationScriptsPartial.cshtml
   └─ Error.cshtml
```

---

## 3. Controller 對應

```text
HomeController          → Views/Home/*
ProductsController      → Views/Products/*
CartController          → Views/Cart/*
CheckoutController      → Views/Checkout/*
OrdersController        → Views/Orders/*
AccountController       → Views/Account/*
AdminController         → Views/Admin/*
```

後台也可以依模組拆 Controller：

```text
AdminDashboardController
AdminProductsController
AdminInventoryController
AdminOrdersController
AdminPaymentsController
AdminShipmentsController
AdminRefundsController
AdminCouponsController
AdminAuditLogsController
```

---

## 4. ViewModel 建議放置位置

```text
MySystem.Application/
└─ ViewModels/
   ├─ Storefront/
   │  ├─ HomeViewModel.cs
   │  ├─ ProductListViewModel.cs
   │  ├─ ProductDetailViewModel.cs
   │  ├─ CartViewModel.cs
   │  └─ OrderListViewModel.cs
   │
   ├─ Admin/
   │  ├─ AdminDashboardViewModel.cs
   │  ├─ AdminProductListViewModel.cs
   │  ├─ AdminOrderListViewModel.cs
   │  └─ AdminAnalyticsViewModel.cs
   │
   └─ Auth/
      ├─ LoginViewModel.cs
      ├─ RegisterViewModel.cs
      └─ ForgotPasswordViewModel.cs
```

若目前專案尚未建立 Application 專案，也可以先暫放在 MVC 專案的 `Models/ViewModels`，但正式分層完成後應移到 Application 層。

---

## 5. 本章總結

`Views/Admin/Index.cshtml` 與 `Views/Admin/Login.cshtml` 可以保留，但前台商品、購物車、訂單與帳號頁需要新增獨立資料夾。Layout 必須拆出 Storefront、Admin、Auth，避免前台與後台共用錯誤的選單與權限邏輯。
