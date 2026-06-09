using System.Security.Claims;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models.ViewModels.Auth;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Services.Pages;

/// <summary>
/// 組裝前台 Razor ViewModel。
/// 目前此 Service 主要支撐頁面切版與流程展示，正式交易邏輯仍應拆到商品、購物車、訂單、付款等專責 Service。
/// </summary>
public sealed class StorefrontPageService : IStorefrontPageService
{
    // TODO: 改由 ProductCategoryService 從 ProductCategories 查詢可公開分類，並加入快取與排序規則。
    private static readonly IReadOnlyList<CategoryCardViewModel> Categories =
    [
        new()
        {
            Name = "水果",
            Description = "當季果物與家庭常備水果。",
            Slug = "fruit",
            VisualClass = "visual-fruit"
        },
        new()
        {
            Name = "蔬菜",
            Description = "葉菜、根莖與料理配菜。",
            Slug = "vegetable",
            VisualClass = "visual-vegetable"
        },
        new()
        {
            Name = "冷藏",
            Description = "需低溫配送的新鮮食材。",
            Slug = "chilled",
            VisualClass = "visual-chilled"
        },
        new()
        {
            Name = "促銷",
            Description = "限時優惠與組合商品。",
            Slug = "promotion",
            VisualClass = "visual-promotion"
        }
    ];

    // TODO: 改由 ProductService / ProductSkuService 查詢正式商品、SKU、售價、上下架與庫存摘要；避免前端信任展示價格。
    private static readonly IReadOnlyList<ProductCardViewModel> ProductCards =
    [
        new()
        {
            ProductId = 1,
            Name = "高山蜜蘋果",
            ShortDescription = "脆甜多汁，適合家庭常備與禮盒搭配。",
            CategoryName = "水果",
            PriceText = "NT$ 189 / 盒",
            PromotionLabel = "當季",
            StockStatusText = "可購買",
            StockStatusVariant = "success",
            VisualClass = "visual-apple"
        },
        new()
        {
            ProductId = 2,
            Name = "有機綠花椰",
            ShortDescription = "清洗分裝，適合快速料理與便當備餐。",
            CategoryName = "蔬菜",
            PriceText = "NT$ 88 / 包",
            PromotionLabel = "冷藏",
            StockStatusText = "低庫存",
            StockStatusVariant = "warning",
            VisualClass = "visual-broccoli"
        },
        new()
        {
            ProductId = 3,
            Name = "綜合沙拉箱",
            ShortDescription = "多款葉菜與小番茄組合，一次備齊輕食餐。",
            CategoryName = "冷藏",
            PriceText = "NT$ 320 / 組",
            PromotionLabel = "組合",
            StockStatusText = "可購買",
            StockStatusVariant = "success",
            VisualClass = "visual-salad"
        },
        new()
        {
            ProductId = 4,
            Name = "週末家庭蔬果箱",
            ShortDescription = "水果與蔬菜搭配，適合三到四人家庭。",
            CategoryName = "促銷",
            PriceText = "NT$ 699 / 箱",
            PromotionLabel = "促銷",
            StockStatusText = "可購買",
            StockStatusVariant = "success",
            VisualClass = "visual-box"
        }
    ];

    public Task<StorefrontHomeViewModel> GetHomeAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = IsAuthenticated(user);

        return Task.FromResult(new StorefrontHomeViewModel
        {
            IsAuthenticated = isAuthenticated,
            // TODO: 改由 CartService 依目前會員查詢購物車數量；匿名使用者不應取用會員購物車。
            CartItemCount = isAuthenticated ? 3 : 0,
            Categories = Categories,
            FeaturedProducts = ProductCards.Select(product => product.WithCanAddToCart(isAuthenticated)).ToArray(),
            PromotionBanners =
            [
                new()
                {
                    Title = "當季蔬果箱",
                    Description = "依照供應狀態由後端計算商品、價格與優惠。",
                    BadgeText = "本週推薦"
                },
                new()
                {
                    Title = "冷藏配送",
                    Description = "結帳時依地址、物流與付款狀態確認配送流程。",
                    BadgeText = "低溫"
                }
            ]
        });
    }

    public Task<ProductListViewModel> GetProductListAsync(
        ClaimsPrincipal user,
        string? keyword,
        string? category,
        int page,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = IsAuthenticated(user);
        var normalizedPage = page < 1 ? 1 : page;
        var products = ProductCards.AsEnumerable();

        // TODO: 正式列表查詢需改成 Repository 分頁查詢，使用 AsNoTracking()，避免載入全部商品後在記憶體篩選。
        if (!string.IsNullOrWhiteSpace(category))
        {
            var selectedCategory = Categories.FirstOrDefault(item =>
                string.Equals(item.Slug, category, StringComparison.OrdinalIgnoreCase));

            if (selectedCategory is not null)
            {
                products = products.Where(product => product.CategoryName == selectedCategory.Name);
            }
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            products = products.Where(product =>
                product.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                || product.ShortDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        var productList = products
            .Select(product => product.WithCanAddToCart(isAuthenticated))
            .ToArray();

        return Task.FromResult(new ProductListViewModel
        {
            IsAuthenticated = isAuthenticated,
            Keyword = keyword ?? string.Empty,
            SelectedCategorySlug = category ?? string.Empty,
            Page = normalizedPage,
            PageSize = 12,
            TotalCount = productList.Length,
            Categories = Categories,
            Products = productList
        });
    }

    public Task<ProductDetailViewModel> GetProductDetailAsync(
        ClaimsPrincipal user,
        int productId,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = IsAuthenticated(user);

        // TODO: 找不到商品時應回傳可由 Controller 轉成 404 的結果，而不是 fallback 到第一筆展示資料。
        var product = ProductCards.FirstOrDefault(item => item.ProductId == productId)
            ?? ProductCards[0];

        return Task.FromResult(new ProductDetailViewModel
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            Description = product.ShortDescription,
            CategoryName = product.CategoryName,
            PriceRange = product.PriceText,
            StockStatusText = product.StockStatusText,
            StockStatusVariant = product.StockStatusVariant,
            VisualClass = product.VisualClass,
            IsAuthenticated = isAuthenticated,
            CanAddToCart = isAuthenticated,
            Highlights =
            [
                "商品價格與優惠由後端提供。",
                "建立訂單時會重新確認庫存與折扣。",
                "付款成功不代表自動出貨。"
            ],
            Skus =
            [
                new()
                {
                    SkuId = product.ProductId * 10 + 1,
                    SkuName = "標準包裝",
                    PriceText = product.PriceText,
                    StockStatusText = product.StockStatusText
                },
                new()
                {
                    SkuId = product.ProductId * 10 + 2,
                    SkuName = "家庭包裝",
                    PriceText = "依正式商品資料計算",
                    StockStatusText = "待確認"
                }
            ]
        });
    }

    public Task<CartViewModel> GetCartAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        // TODO: 改由 CartService 讀取目前會員購物車，並重新計算價格、優惠與庫存狀態。
        return Task.FromResult(new CartViewModel
        {
            Items =
            [
                new()
                {
                    ProductId = 1,
                    ProductName = "高山蜜蘋果",
                    SkuName = "標準包裝",
                    Quantity = 1,
                    UnitPriceText = "NT$ 189",
                    LineTotalText = "NT$ 189",
                    StockStatusText = "可出貨"
                },
                new()
                {
                    ProductId = 3,
                    ProductName = "綜合沙拉箱",
                    SkuName = "家庭包裝",
                    Quantity = 1,
                    UnitPriceText = "NT$ 320",
                    LineTotalText = "NT$ 320",
                    StockStatusText = "冷藏配送"
                }
            ],
            SubtotalText = "NT$ 509",
            DiscountTotalText = "由後端促銷服務計算",
            EstimatedTotalText = "結帳時重新計算",
            CanCheckout = true
        });
    }

    public Task<CheckoutViewModel> GetCheckoutAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        // TODO: 改由 CheckoutService / OrderDraftService 建立結帳摘要；送出訂單時必須在 Service 交易中重新驗價與保留庫存。
        return Task.FromResult(new CheckoutViewModel
        {
            Items =
            [
                new()
                {
                    ProductId = 1,
                    ProductName = "高山蜜蘋果",
                    SkuName = "標準包裝",
                    Quantity = 1,
                    UnitPriceText = "NT$ 189",
                    LineTotalText = "NT$ 189",
                    StockStatusText = "可出貨"
                }
            ],
            ShippingInfo = new ShippingInfoViewModel
            {
                RecipientName = string.Empty,
                PhoneNumber = string.Empty,
                Address = string.Empty
            },
            PaymentMethods =
            [
                new()
                {
                    Code = "credit-card",
                    DisplayName = "信用卡",
                    Description = "付款結果以金流 Callback 驗簽後為準。"
                },
                new()
                {
                    Code = "atm",
                    DisplayName = "ATM 轉帳",
                    Description = "入帳後更新付款狀態。"
                }
            ],
            OrderSummary = new OrderSummaryViewModel
            {
                SubtotalText = "由後端重新計算",
                DiscountTotalText = "由優惠服務計算",
                ShippingFeeText = "依物流服務計算",
                PayableTotalText = "送出訂單後確認"
            }
        });
    }

    public Task<CheckoutCompleteViewModel> GetCheckoutCompleteAsync(
        ClaimsPrincipal user,
        string? orderNumber,
        CancellationToken cancellationToken)
    {
        // TODO: 正式完成頁需依目前會員查詢訂單，確認資料擁有者，避免使用 query string 顯示他人訂單。
        return Task.FromResult(new CheckoutCompleteViewModel
        {
            OrderNumber = string.IsNullOrWhiteSpace(orderNumber) ? "尚未建立正式訂單" : orderNumber,
            OrderStatusText = "等待付款確認",
            PaymentStatusText = "待付款",
            NextStepText = "付款完成後訂單進入 Paid，仍需後台確認出貨。"
        });
    }

    public Task<OrderListViewModel> GetOrderListAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        // TODO: 改由 OrderService 只查詢目前會員自己的訂單，並加入分頁與狀態篩選。
        return Task.FromResult(new OrderListViewModel
        {
            Orders =
            [
                new()
                {
                    OrderId = 1,
                    OrderNumber = "ORD-DEMO-001",
                    CreatedAtText = "展示資料",
                    TotalText = "由訂單服務提供",
                    OrderStatusText = "待付款",
                    PaymentStatusText = "未付款",
                    ShipmentStatusText = "未出貨"
                }
            ]
        });
    }

    public Task<OrderDetailViewModel> GetOrderDetailAsync(
        ClaimsPrincipal user,
        int orderId,
        CancellationToken cancellationToken)
    {
        // TODO: 改由 OrderService 查詢訂單詳細，Service 必須確認 Order.UserId 等於目前會員。
        return Task.FromResult(new OrderDetailViewModel
        {
            OrderNumber = $"ORD-DEMO-{orderId:000}",
            OrderStatusText = "待付款",
            PaymentStatusText = "未付款",
            ShipmentStatusText = "未出貨",
            TotalText = "由訂單服務提供",
            Items =
            [
                new()
                {
                    ProductName = "高山蜜蘋果",
                    SkuName = "標準包裝",
                    Quantity = 1,
                    UnitPriceText = "NT$ 189",
                    LineTotalText = "NT$ 189"
                }
            ],
            Timeline =
            [
                new()
                {
                    Title = "訂單建立",
                    Description = "建立訂單時應保留庫存。",
                    OccurredAtText = "展示資料"
                },
                new()
                {
                    Title = "等待付款",
                    Description = "付款 Callback 驗簽成功後才更新付款狀態。",
                    OccurredAtText = "展示資料"
                }
            ]
        });
    }

    public Task<MemberProfileViewModel> GetMemberProfileAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        // TODO: 改由 MemberService 查詢會員資料；不可將 PasswordHash、SecurityStamp 或內部稽核欄位放入 ViewModel。
        return Task.FromResult(new MemberProfileViewModel
        {
            DisplayName = user.Identity?.Name ?? "會員",
            Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            PhoneNumber = string.Empty,
            DefaultShippingAddress = string.Empty
        });
    }

    private static bool IsAuthenticated(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }
}

file static class ProductCardViewModelExtensions
{
    public static ProductCardViewModel WithCanAddToCart(
        this ProductCardViewModel product,
        bool canAddToCart)
    {
        return new ProductCardViewModel
        {
            ProductId = product.ProductId,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            CategoryName = product.CategoryName,
            PriceText = product.PriceText,
            PromotionLabel = product.PromotionLabel,
            StockStatusText = product.StockStatusText,
            StockStatusVariant = product.StockStatusVariant,
            VisualClass = product.VisualClass,
            CanAddToCart = canAddToCart
        };
    }
}
