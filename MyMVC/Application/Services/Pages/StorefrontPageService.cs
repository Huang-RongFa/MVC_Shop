using System.Security.Claims;
using MyWeb.Application.DTOs.Storefront.Products;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models.ViewModels.Auth;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Services.Pages;

/// <summary>
/// 組裝前台 Razor ViewModel。
/// 商品、分類、價格與庫存由 Catalog/Cart Service 提供，避免 View 使用靜態商品假資料。
/// </summary>
public sealed class StorefrontPageService : IStorefrontPageService
{
    private readonly ICartService _cartService;
    private readonly IProductCatalogQueryService _productCatalogQueryService;

    public StorefrontPageService(
        ICartService cartService,
        IProductCatalogQueryService productCatalogQueryService)
    {
        _cartService = cartService;
        _productCatalogQueryService = productCatalogQueryService;
    }

    // 農場故事目前是前台內容頁的展示資料，未參與訂單、庫存、價格或權限決策。
    // 正式內容管理完成後，應改由內容/供應商模組提供，View 仍只接收 ViewModel。
    private static readonly IReadOnlyList<FarmerStoryCardViewModel> Farmers =
    [
        new()
        {
            FarmerId = 1,
            Name = "陳明輝",
            Avatar = "👨‍🌾",
            Region = "台南玉井",
            RegionTag = "台南",
            VisualClass = "farmer-mango",
            ProduceEmoji = "🥭",
            YearsOfExperience = 22,
            Story = "陳明輝在玉井種了二十多年的愛文芒果。從父親那代傳下來的園子，如今也開始讓兒子一起打理。他說，好的芒果靠的是耐心，等果子自己決定什麼時候熟。",
            ProduceTags = ["🥭 愛文芒果", "🍍 鳳梨", "🫙 手工芒果醬"],
            Certifications = ["有機", "CAS"],
            OrderCountText = "1,240",
            RatingText = "4.97"
        },
        new()
        {
            FarmerId = 2,
            Name = "林秀珍",
            Avatar = "👩‍🌾",
            Region = "苗栗大湖",
            RegionTag = "苗栗",
            VisualClass = "farmer-strawberry",
            ProduceEmoji = "🍓",
            YearsOfExperience = 15,
            Story = "林秀珍是大湖少數堅持不套袋種植草莓的農夫。她說日照才是甜度的關鍵，她的草莓比市售的小一圈，但香氣是普通草莓的兩倍。",
            ProduceTags = ["🍓 大湖草莓", "🫐 野生藍莓", "🍓 草莓果醬"],
            Certifications = ["有機"],
            OrderCountText = "867",
            RatingText = "4.98"
        },
        new()
        {
            FarmerId = 3,
            Name = "張志遠",
            Avatar = "🧑‍🌾",
            Region = "南投埔里",
            RegionTag = "南投",
            VisualClass = "farmer-mushroom",
            ProduceEmoji = "🍄",
            YearsOfExperience = 9,
            Story = "前工程師轉行種菇，張志遠把精準控溫技術帶進菇棚。他的菌絲來自日本長野，在埔里的霧氣中長出完全不輸進口品的風味。",
            ProduceTags = ["🍄 松茸", "🫘 牛肝菌", "🌾 雪白菇", "🍄 黑木耳"],
            Certifications = ["CAS"],
            OrderCountText = "543",
            RatingText = "4.96"
        },
        new()
        {
            FarmerId = 4,
            Name = "王美慧",
            Avatar = "👩‍🌾",
            Region = "雲林西螺",
            RegionTag = "雲林",
            VisualClass = "farmer-vegetable",
            ProduceEmoji = "🥦",
            YearsOfExperience = 18,
            Story = "西螺醬油的故鄉也盛產蔬菜。王美慧的家族三代務農，她率先在西螺申請有機轉型，說服周圍五個農家一起改變。",
            ProduceTags = ["🥦 花椰菜", "🥬 高麗菜", "🫑 青椒", "🥕 紅蘿蔔", "🧅 洋蔥"],
            Certifications = ["有機", "CAS"],
            OrderCountText = "2,100",
            RatingText = "4.95"
        },
        new()
        {
            FarmerId = 5,
            Name = "李國豪",
            Avatar = "👨‍🌾",
            Region = "屏東縣",
            RegionTag = "屏東",
            VisualClass = "farmer-citrus",
            ProduceEmoji = "🍋",
            YearsOfExperience = 11,
            Story = "屏東的陽光讓檸檬的酸更立體。李國豪採用自然農法，不打除草劑，讓雜草在果樹旁自然生長。",
            ProduceTags = ["🍋 無毒檸檬", "🍊 茂谷柑", "🍈 文旦"],
            Certifications = ["農藥檢驗"],
            OrderCountText = "789",
            RatingText = "4.93"
        },
        new()
        {
            FarmerId = 6,
            Name = "蔡慧如",
            Avatar = "👩‍🌾",
            Region = "梨山",
            RegionTag = "梨山",
            VisualClass = "farmer-mountain",
            ProduceEmoji = "🍎",
            YearsOfExperience = 27,
            Story = "海拔 2000 公尺的梨山冬天會結霜。蔡慧如的蘋果在嚴寒中長得慢，也因此積累了更多糖分。",
            ProduceTags = ["🍎 梨山蘋果", "🍐 新興梨", "🍑 水蜜桃"],
            Certifications = ["CAS", "高山產區"],
            OrderCountText = "1,560",
            RatingText = "4.99"
        }
    ];

    public async Task<StorefrontHomeViewModel> GetHomeAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = IsAuthenticated(user);
        var canShopAsCustomer = CanShopAsCustomer(user);
        // 未登入或非 Customer 身分不讀取購物車，避免後台 Cookie 進入前台時誤顯購物狀態。
        var cart = await GetCurrentCartAsync(user, canShopAsCustomer, cancellationToken);
        var cartQuantities = GetCurrentCartQuantities(cart);
        var featuredProducts = ApplyCartState(
            await _productCatalogQueryService.GetFeaturedProductsAsync(8, cancellationToken),
            cartQuantities,
            canShopAsCustomer);

        return new StorefrontHomeViewModel
        {
            IsAuthenticated = isAuthenticated,
            CanShopAsCustomer = canShopAsCustomer,
            CartItemCount = cart.Items.Sum(item => item.Quantity),
            Categories = await _productCatalogQueryService.GetCategoriesAsync(cancellationToken),
            FeaturedProducts = featuredProducts,
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
        };
    }

    public async Task<ProductListViewModel> GetProductListAsync(
        ClaimsPrincipal user,
        string? keyword,
        string? category,
        string? origin,
        string? certification,
        int? minPrice,
        int? maxPrice,
        string? sort,
        string? view,
        int page,
        CancellationToken cancellationToken)
    {
        var isAuthenticated = IsAuthenticated(user);
        var canShopAsCustomer = CanShopAsCustomer(user);
        var normalizedPage = page < 1 ? 1 : page;
        var categories = await _productCatalogQueryService.GetCategoriesAsync(cancellationToken);
        var selectedCategory = categories.FirstOrDefault(item =>
            string.Equals(item.Slug, category, StringComparison.OrdinalIgnoreCase));
        var normalizedSort = NormalizeSort(sort);
        var normalizedView = string.Equals(view, "list", StringComparison.OrdinalIgnoreCase)
            ? "list"
            : "grid";

        // 商品列表的篩選值先轉成受控 criteria，讓資料查詢層只接收已正規化的查詢意圖。
        var result = await _productCatalogQueryService.SearchProductsAsync(
            new ProductCatalogSearchCriteria(
                Keyword: keyword?.Trim() ?? string.Empty,
                CategoryCode: selectedCategory?.Slug ?? string.Empty,
                OriginName: origin?.Trim() ?? string.Empty,
                Certification: certification?.Trim() ?? string.Empty,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                Sort: normalizedSort,
                Page: normalizedPage,
                PageSize: 12,
                FeaturedOnly: false),
            cancellationToken);
        var cart = await GetCurrentCartAsync(user, canShopAsCustomer, cancellationToken);

        return new ProductListViewModel
        {
            IsAuthenticated = isAuthenticated,
            CanShopAsCustomer = canShopAsCustomer,
            ListTitle = selectedCategory is null ? "商品列表" : $"{selectedCategory.Name}商品",
            Keyword = keyword ?? string.Empty,
            SelectedCategorySlug = selectedCategory?.Slug ?? string.Empty,
            SelectedOrigin = origin?.Trim() ?? string.Empty,
            SelectedCertification = certification?.Trim() ?? string.Empty,
            Sort = normalizedSort,
            ViewMode = normalizedView,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            Page = normalizedPage,
            PageSize = 12,
            TotalCount = result.TotalCount,
            Categories = categories,
            Origins = result.Origins,
            Certifications = result.Certifications,
            Products = ApplyCartState(result.Products, GetCurrentCartQuantities(cart), canShopAsCustomer)
        };
    }

    public async Task<ProductDetailViewModel?> GetProductDetailAsync(
        ClaimsPrincipal user,
        int productId,
        CancellationToken cancellationToken)
    {
        var detail = await _productCatalogQueryService.GetProductDetailAsync(productId, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        var isAuthenticated = IsAuthenticated(user);
        var canShopAsCustomer = CanShopAsCustomer(user);
        var cart = await GetCurrentCartAsync(user, canShopAsCustomer, cancellationToken);
        var currentCartQuantity = GetCurrentCartQuantities(cart).GetValueOrDefault(detail.ProductId);

        // CanAddToCart 是 UI 狀態；實際加入購物車時 CartService 仍會重新檢查會員身分與庫存。
        return new ProductDetailViewModel
        {
            ProductId = detail.ProductId,
            ProductName = detail.ProductName,
            Description = detail.Description,
            CategoryName = detail.CategoryName,
            OriginName = detail.OriginName,
            FarmerName = detail.FarmerName,
            PriceRange = detail.PriceRange,
            UnitText = detail.UnitText,
            StockStatusText = detail.StockStatusText,
            StockStatusVariant = detail.StockStatusVariant,
            AvailableStockQuantity = detail.AvailableStockQuantity,
            CurrentCartQuantity = currentCartQuantity,
            VisualClass = detail.VisualClass,
            IsAuthenticated = isAuthenticated,
            CanShopAsCustomer = canShopAsCustomer,
            CanAddToCart = canShopAsCustomer && detail.AvailableStockQuantity > currentCartQuantity,
            Skus = detail.Skus,
            Highlights = detail.Highlights,
            Certifications = detail.Certifications
        };
    }

    public Task<FarmerStoryPageViewModel> GetFarmerStoriesAsync(
        string? filter,
        CancellationToken cancellationToken)
    {
        var normalizedFilter = string.IsNullOrWhiteSpace(filter) ? "全部" : filter.Trim();
        var farmers = normalizedFilter == "全部"
            ? Farmers
            : Farmers
                .Where(farmer =>
                    farmer.RegionTag.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase)
                    || farmer.Region.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase)
                    || normalizedFilter.Contains(farmer.Region, StringComparison.OrdinalIgnoreCase)
                    || normalizedFilter.Contains(farmer.RegionTag, StringComparison.OrdinalIgnoreCase)
                    || farmer.Name.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase)
                    || farmer.ProduceTags.Any(tag => tag.Contains(normalizedFilter, StringComparison.OrdinalIgnoreCase))
                    || farmer.Certifications.Any(item => item.Equals(normalizedFilter, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

        return Task.FromResult(new FarmerStoryPageViewModel
        {
            SelectedFilter = normalizedFilter,
            Filters =
            [
                new() { Label = "全部農場", Value = "全部" },
                new() { Label = "台南", Value = "台南" },
                new() { Label = "苗栗", Value = "苗栗" },
                new() { Label = "南投", Value = "南投" },
                new() { Label = "雲林", Value = "雲林" },
                new() { Label = "屏東", Value = "屏東" },
                new() { Label = "梨山", Value = "梨山" },
                new() { Label = "有機認證", Value = "有機" },
                new() { Label = "CAS 優良", Value = "CAS" }
            ],
            Farmers = farmers,
            Promises =
            [
                new()
                {
                    Icon = "🤝",
                    Title = "產地直簽契約",
                    Description = "與合作農家建立長期採購關係，降低中間流通成本，也讓農夫能穩定投入品質。"
                },
                new()
                {
                    Icon = "🔬",
                    Title = "第三方農藥檢驗",
                    Description = "高風險品項上架前保留檢驗與批次追蹤規劃，正式上線後應連動商品批號資料。"
                },
                new()
                {
                    Icon = "💰",
                    Title = "公平收購定價",
                    Description = "以可追蹤的商品與供應來源呈現價格，不讓前端自行決定售價或折扣結果。"
                }
            ],
            Regions =
            [
                new() { Emoji = "🏔️", Name = "梨山・武陵", CountText = "8 個農場" },
                new() { Emoji = "🌊", Name = "苗栗大湖", CountText = "12 個農場" },
                new() { Emoji = "🌾", Name = "雲林西螺", CountText = "21 個農場" },
                new() { Emoji = "🌿", Name = "南投埔里", CountText = "15 個農場" },
                new() { Emoji = "☀️", Name = "台南玉井", CountText = "18 個農場" },
                new() { Emoji = "🌺", Name = "屏東縣", CountText = "24 個農場" },
                new() { Emoji = "🫐", Name = "彰化田中", CountText = "16 個農場" },
                new() { Emoji = "🍊", Name = "台東縣", CountText = "13 個農場" }
            ]
        });
    }

    public Task<CartViewModel> GetCartAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return _cartService.GetCartAsync(user, cancellationToken);
    }

    public async Task<CheckoutViewModel> GetCheckoutAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(user, cancellationToken);

        // 結帳頁目前只呈現購物車摘要；正式建立訂單時不可沿用前端顯示金額，需由 OrderService 重新計算。
        return new CheckoutViewModel
        {
            Items = cart.Items,
            ShippingInfo = new ShippingInfoViewModel(),
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
                SubtotalText = cart.SubtotalText,
                DiscountTotalText = cart.DiscountTotalText,
                ShippingFeeText = "依物流服務計算",
                PayableTotalText = cart.EstimatedTotalText
            }
        };
    }

    public Task<CheckoutCompleteViewModel> GetCheckoutCompleteAsync(
        ClaimsPrincipal user,
        string? orderNumber,
        CancellationToken cancellationToken)
    {
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
        return Task.FromResult(new OrderListViewModel());
    }

    public Task<OrderDetailViewModel> GetOrderDetailAsync(
        ClaimsPrincipal user,
        int orderId,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new OrderDetailViewModel
        {
            OrderNumber = $"ORD-{orderId:000}",
            OrderStatusText = "尚未串接正式訂單查詢",
            PaymentStatusText = "尚未串接正式付款查詢",
            ShipmentStatusText = "尚未串接正式物流查詢",
            TotalText = "NT$ 0"
        });
    }

    public Task<MemberProfileViewModel> GetMemberProfileAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new MemberProfileViewModel
        {
            DisplayName = user.Identity?.Name ?? "會員",
            Email = user.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            PhoneNumber = string.Empty,
            DefaultShippingAddress = string.Empty
        });
    }

    private static string NormalizeSort(string? sort)
    {
        // 與 ProductCatalogQueryService 保持相同排序語意，避免頁面 query string 出現未支援值。
        return sort switch
        {
            "new" => "new",
            "price-asc" => "price-asc",
            "price-desc" => "price-desc",
            "rating" => "rating",
            _ => "popular"
        };
    }

    private static bool IsAuthenticated(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }

    private static bool CanShopAsCustomer(ClaimsPrincipal user)
    {
        // 只有 Customer 可進入購買流程；Admin/Staff 即使登入也不應具有前台購買資格。
        return IsAuthenticated(user)
            && user.Claims.Any(claim =>
                (string.Equals(claim.Type, "user_type", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(claim.Type, "UserType", StringComparison.OrdinalIgnoreCase))
                && string.Equals(claim.Value, "Customer", StringComparison.OrdinalIgnoreCase));
    }

    private async Task<CartViewModel> GetCurrentCartAsync(
        ClaimsPrincipal user,
        bool canShopAsCustomer,
        CancellationToken cancellationToken)
    {
        return canShopAsCustomer
            ? await _cartService.GetCartAsync(user, cancellationToken)
            : new CartViewModel();
    }

    private static IReadOnlyDictionary<long, int> GetCurrentCartQuantities(CartViewModel cart)
    {
        return cart.Items
            .GroupBy(item => item.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(item => item.Quantity));
    }

    private static IReadOnlyList<ProductCardViewModel> ApplyCartState(
        IReadOnlyList<ProductCardViewModel> products,
        IReadOnlyDictionary<long, int> cartQuantities,
        bool canShopAsCustomer)
    {
        return products
            .Select(product => ApplyCartState(product, cartQuantities, canShopAsCustomer))
            .ToArray();
    }

    private static ProductCardViewModel ApplyCartState(
        ProductCardViewModel product,
        IReadOnlyDictionary<long, int> cartQuantities,
        bool canShopAsCustomer)
    {
        var currentCartQuantity = cartQuantities.GetValueOrDefault(product.ProductId);

        // 以新的 ViewModel 實例回填目前購物車狀態，避免修改 Catalog Service 回傳的共用模型。
        return new ProductCardViewModel
        {
            ProductId = product.ProductId,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            CategoryName = product.CategoryName,
            OriginName = product.OriginName,
            FarmerName = product.FarmerName,
            PriceText = product.PriceText,
            PriceAmount = product.PriceAmount,
            UnitText = product.UnitText,
            PromotionLabel = product.PromotionLabel,
            StockStatusText = product.StockStatusText,
            StockStatusVariant = product.StockStatusVariant,
            AvailableStockQuantity = product.AvailableStockQuantity,
            CurrentCartQuantity = currentCartQuantity,
            VisualClass = product.VisualClass,
            RatingText = product.RatingText,
            PublishedAt = product.PublishedAt,
            Certifications = product.Certifications,
            CanAddToCart = canShopAsCustomer && product.AvailableStockQuantity > currentCartQuantity
        };
    }
}
