# 07｜切版轉換成 cshtml 步驟

## 1. 轉換總流程

```text
原始 HTML
  ↓
拆出共用 Layout
  ↓
拆出 Header / Mobile Menu / Theme Toggle Partial
  ↓
保留各頁 main-content
  ↓
改成 Razor 語法與 Tag Helper
  ↓
建立 ViewModel
  ↓
Controller 傳資料到 View
  ↓
移除假資料
  ↓
加入權限與驗收
```

---

## 2. 第一步：搬移 CSS / JS

```text
1. 將 templatemo-daynight-style.css 複製為 wwwroot/css/daynight.css
2. 將 templatemo-daynight-script.js 複製為 wwwroot/js/daynight.js
3. 在 Layout 引入 CSS / JS
4. 確認首頁可以正常顯示樣式
```

---

## 3. 第二步：拆 Layout

從原始 HTML 中拆出：

```text
<head>
<body>
<nav class="top-nav">
<div class="mobile-menu">
<script>
```

建議 Layout：

```text
_StorefrontLayout.cshtml
_AdminLayout.cshtml
_AuthLayout.cshtml
```

`@RenderBody()` 放在原本的：

```html
<main class="main-content">
    @RenderBody()
</main>
```

---

## 4. 第三步：改連結

將 HTML 連結：

```html
<a href="index.html">儀表板</a>
```

改成 Tag Helper：

```html
<a asp-controller="Admin" asp-action="Index">儀表板</a>
```

前台商品：

```html
<a asp-controller="Products" asp-action="Index">商品</a>
```

---

## 5. 第四步：建立 ViewModel

不要讓 View 直接查資料，也不要把 Entity 直接丟給 View。

範例：

```csharp
public class StorefrontHomeViewModel
{
    public IReadOnlyList<ProductCardViewModel> FeaturedProducts { get; set; } = [];
    public IReadOnlyList<CategoryCardViewModel> Categories { get; set; } = [];
}
```

後台 Dashboard：

```csharp
public class AdminDashboardViewModel
{
    public decimal TodayRevenue { get; set; }
    public int TodayOrderCount { get; set; }
    public int PendingShipmentCount { get; set; }
    public int LowStockProductCount { get; set; }
}
```

---

## 6. 第五步：替換假資料

原本：

```html
<div class="stat-value">$48,250</div>
```

改成：

```html
<div class="stat-value">@Model.TodayRevenue.ToString("N0")</div>
```

商品卡片：

```razor
@foreach (var product in Model.FeaturedProducts)
{
    <article class="template-card">
        <img src="@product.ImageUrl" alt="@product.Name" class="template-thumb" />
        <div class="template-info">
            <h3>@product.Name</h3>
            <p>@product.ShortDescription</p>
            <div class="template-tags">
                <span class="template-tag">@product.CategoryName</span>
            </div>
        </div>
    </article>
}
```

---

## 7. 第六步：加入權限顯示

後台選單可依權限顯示：

```razor
@if (User.HasClaim("Permission", "Products.Read"))
{
    <a asp-controller="AdminProducts" asp-action="Index">商品管理</a>
}
```

注意：這只是 UX，Controller 與 Service 仍必須檢查權限。

---

## 8. 第七步：加入安全處理

```text
□ 表單使用 Anti-forgery Token
□ POST Action 加 [ValidateAntiForgeryToken]
□ 後台 Controller 加 [Authorize]
□ 後台高風險操作檢查 Permission
□ 不使用未清理資料輸出到 Html.Raw
□ 不把使用者輸入塞進 innerHTML
```

---

## 9. 本章總結

切版不是把 `.html` 改副檔名成 `.cshtml`。正式做法是拆 Layout、拆 Partial、改路由、建立 ViewModel、移除假資料、加入權限與安全防護。
