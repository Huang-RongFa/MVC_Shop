# 05｜前台與後台 Vue 正式部署流程

## 1. 本章目的

本章規劃兩個 Vue App：

```text
storefront：前台顧客使用
admin：後台管理員與員工使用
```

兩個 App 目的不同，因此路由、API、UI、權限、部署與資安設定都應分開。

---

## 2. 前端技術堆疊

```text
Vue
Vue Router
Pinia
Axios 或 Fetch
Bootstrap 5
SweetAlert2
Vite
RWD
RESTful API
```

正式部署時，前端應視為「可被使用者檢視與修改的程式碼」。所以前端不可保存秘密，也不可作為真正權限來源。

---

## 3. 專案結構規劃

```text
frontend/
├─ storefront/
│  ├─ src/
│  │  ├─ api/
│  │  │  ├─ apiClient.js
│  │  │  ├─ authApi.js
│  │  │  ├─ productApi.js
│  │  │  ├─ cartApi.js
│  │  │  ├─ orderApi.js
│  │  │  └─ paymentApi.js
│  │  ├─ views/
│  │  ├─ components/
│  │  ├─ router/
│  │  ├─ stores/
│  │  └─ main.js
│  ├─ .env.development
│  ├─ .env.production
│  └─ package.json
│
└─ admin/
   ├─ src/
   │  ├─ api/
   │  │  ├─ apiClient.js
   │  │  ├─ authApi.js
   │  │  ├─ userApi.js
   │  │  ├─ productApi.js
   │  │  ├─ inventoryApi.js
   │  │  ├─ orderApi.js
   │  │  ├─ paymentApi.js
   │  │  ├─ shipmentApi.js
   │  │  ├─ refundApi.js
   │  │  ├─ couponApi.js
   │  │  └─ auditApi.js
   │  ├─ views/
   │  ├─ components/
   │  ├─ router/
   │  ├─ stores/
   │  └─ main.js
   ├─ .env.development
   ├─ .env.production
   └─ package.json
```

---

## 4. Storefront App 頁面

| 前台頁面 | 主要 API | 說明 |
|---|---|---|
| HomeView | `/api/storefront/products` | 首頁商品與促銷 |
| ProductListView | `/api/storefront/products` | 商品列表、分類、搜尋 |
| ProductDetailView | `/api/storefront/products/{id}` | 商品詳細與 SKU |
| CartView | `/api/storefront/cart` | 購物車 |
| CheckoutView | `/api/storefront/orders` | 建立訂單 |
| PaymentView | `/api/storefront/orders/{id}/payments` | 建立付款 |
| MyOrdersView | `/api/storefront/me/orders` | 會員自己的訂單 |
| MyOrderDetailView | `/api/storefront/me/orders/{id}` | 會員自己的訂單明細 |
| LoginView | `/api/storefront/auth/login` | 前台登入 |
| RegisterView | `/api/storefront/auth/register` | 前台註冊 |
| ForgotPasswordView | `/api/storefront/auth/forgot-password` | 忘記密碼 |

前台 API 的重點是「顧客只能操作自己的資料」。

---

## 5. Admin App 頁面

| 後台頁面 | 主要 API | 說明 |
|---|---|---|
| AdminLoginView | `/api/admin/auth/login` | 後台登入 |
| DashboardView | `/api/admin/dashboard` | 後台統計 |
| UserListView | `/api/admin/users` | 使用者管理 |
| RolePermissionView | `/api/admin/roles` | 角色權限 |
| ProductListView | `/api/admin/products` | 商品管理 |
| ProductSkuView | `/api/admin/products/{id}/skus` | SKU 管理 |
| InventoryStockView | `/api/admin/inventory/stocks` | 庫存查詢 |
| InventoryTransactionView | `/api/admin/inventory/transactions` | 庫存異動 |
| OrderListView | `/api/admin/orders` | 訂單管理 |
| OrderDetailView | `/api/admin/orders/{id}` | 訂單詳細 |
| PaymentListView | `/api/admin/payments` | 付款管理 |
| ShipmentListView | `/api/admin/shipments` | 出貨管理 |
| RefundListView | `/api/admin/refunds` | 退款管理 |
| CouponListView | `/api/admin/coupons` | 優惠券管理 |
| PromotionListView | `/api/admin/promotions` | 促銷管理 |
| AuditLogView | `/api/admin/audit/*` | 稽核查詢 |

後台 API 的重點是「依角色與權限控制管理操作」。

---

## 6. API Client 規劃

正式上線使用 HttpOnly Secure Cookie 時，前端不需要手動保存高權限 Token。

`apiClient.js` 建議集中設定：

```text
baseURL
withCredentials: true
timeout
X-CSRF-TOKEN Header
401 自動導回登入頁
403 顯示無權限
409 顯示資料衝突
500 顯示通用錯誤訊息
Loading 狀態
錯誤訊息統一處理
```

前端不可：

```text
□ 保存資料庫密碼
□ 保存金流密鑰
□ 保存 Cookie / Token 簽章金鑰
□ 長期保存高權限 JWT 到 localStorage
□ 相信自己 store 裡的 Role 就直接顯示敏感資料
```

---

## 7. CSRF Token 前端流程

建議流程：

```text
1. App 啟動時呼叫 /api/csrf-token
2. 後端回傳或設定 XSRF Token
3. 前端 API Client 將 Token 放入 Header
4. 所有 POST / PUT / PATCH / DELETE 自動帶 Header
5. 後端驗證 Token
```

注意：

```text
□ XSRF Token 不是登入 Cookie
□ XSRF Token 可以被前端讀取，但不能當成登入憑證
□ 登入 Cookie 必須 HttpOnly，前端不應讀取
```

---

## 8. Store 規劃

Storefront App：

```text
authStore：前台登入狀態與會員資料
cartStore：購物車狀態
checkoutStore：結帳流程
productStore：商品查詢條件
notificationStore：提示訊息
```

Admin App：

```text
authStore：後台登入狀態、角色、權限
layoutStore：側邊選單、載入狀態
productStore：商品維護狀態
orderStore：訂單查詢條件
permissionStore：權限判斷輔助
notificationStore：提示訊息
```

Store 只能作為前端狀態管理，不是權限真實來源。

---

## 9. Router Guard

前台：

```text
□ 未登入不可進入我的訂單、結帳頁
□ 已登入者進登入頁可導回首頁或會員中心
□ 401 時清空 authStore 並導回登入頁
```

後台：

```text
□ 未登入不可進入後台頁面
□ 沒有權限的頁面不顯示選單
□ 直接輸入 URL 仍需後端 API 檢查權限
□ 403 顯示無權限頁面
```

---

## 10. XSS 與前端安全

前端要避免將未清理內容直接放入 HTML。

檢查：

```text
□ 避免 v-html 顯示使用者輸入
□ 商品描述若允許 HTML，需後端清理白名單
□ 錯誤訊息不顯示 Stack Trace
□ 不把 API 原始錯誤完整印在畫面
□ 不把機密寫入 console.log
□ npm 套件定期檢查弱點
□ 前端 Build 後不包含 .env 機密
```

---

## 11. 前台 SEO 與可用性

若前台商品需要搜尋引擎曝光，純 SPA 可能 SEO 較弱。

初期可先：

```text
□ 商品頁 title / meta description 動態設定
□ 商品圖片 alt 文字
□ Open Graph 基本設定
□ sitemap.xml
□ robots.txt
□ 404 頁面
□ Loading 與錯誤狀態設計
```

若 SEO 需求高，再評估：

```text
SSR
SSG
Nuxt
預渲染
```

---

## 12. 部署規劃

前台與後台可分開部署：

```text
https://www.example.com
https://admin.example.com
https://api.example.com
```

或初期簡化為同站部署：

```text
https://www.example.com
https://www.example.com/admin
https://www.example.com/api
```

部署前檢查：

```text
□ .env.production 只放公開設定，例如 API Base URL
□ 不把秘密寫入前端環境變數
□ API Base URL 指向正式 API
□ 前後台 Build 成功
□ CORS 已加入正式網域
□ Cookie Domain / SameSite 已測試
□ 401 / 403 / 500 顯示正常
□ 登入、下單、後台操作流程可用
```

---

## 13. 本章總結

正式架構應拆成 Storefront 與 Admin 兩個 Vue App。前台重視購物體驗、SEO 與使用者流程；後台重視權限、稽核與資料管理。前端可做 UX 控制，但真正的價格、權限、資料擁有者與交易規則都必須由後端重新驗證。
