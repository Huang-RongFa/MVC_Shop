using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.Commerce;
using MyWeb.Application.Security;

namespace MyWeb.Application.Services.Admin.Commerce;

/// <summary>
/// 後台商品、SKU、庫存與訂單列表的商業流程。
/// 這一層負責 Permission 再檢查、輸入正規化、狀態白名單與圖片路徑安全；
/// Repository 則只依已驗證的請求執行資料存取與交易寫入。
/// </summary>
public sealed class AdminCommerceService : IAdminCommerceService
{
    // 後台列表使用固定 pageSize 選項，避免管理查詢被任意大分頁拖慢。
    private static readonly HashSet<int> AllowedPageSizes = [10, 20, 50];
    private static readonly HashSet<string> ValidProductStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Active",
        "Inactive",
        "Archived"
    };

    private static readonly HashSet<string> ValidSkuStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active",
        "Inactive"
    };

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private readonly IAdminCommerceRepository _adminCommerceRepository;

    public AdminCommerceService(IAdminCommerceRepository adminCommerceRepository)
    {
        _adminCommerceRepository = adminCommerceRepository;
    }

    public Task<AdminProductListResponse> GetProductsAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsRead);
        var (normalizedPage, normalizedPageSize) = NormalizePaging(page, pageSize);

        return _adminCommerceRepository.GetProductsAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken);
    }

    public Task<AdminProductEditResponse> GetProductCreateOptionsAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsWrite);
        return _adminCommerceRepository.GetProductCreateOptionsAsync(cancellationToken);
    }

    public Task<AdminProductEditResponse> GetProductForEditAsync(
        ClaimsPrincipal adminUser,
        long productId,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsWrite);

        if (productId <= 0)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_ID_INVALID", "商品編號不正確。");
        }

        return _adminCommerceRepository.GetProductForEditAsync(productId, cancellationToken);
    }

    public Task<AdminProductMutationResponse> CreateProductAsync(
        ClaimsPrincipal adminUser,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsWrite);
        var actorUserId = GetActorUserId(adminUser);
        var normalizedRequest = NormalizeProductRequest(request);

        return _adminCommerceRepository.CreateProductAsync(
            actorUserId,
            normalizedRequest,
            requestContext,
            cancellationToken);
    }

    public Task<AdminProductMutationResponse> UpdateProductAsync(
        ClaimsPrincipal adminUser,
        long productId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsWrite);

        if (productId <= 0)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_ID_INVALID", "商品編號不正確。");
        }

        var actorUserId = GetActorUserId(adminUser);
        var normalizedRequest = NormalizeProductRequest(request);

        return _adminCommerceRepository.UpdateProductAsync(
            actorUserId,
            productId,
            normalizedRequest,
            requestContext,
            cancellationToken);
    }

    public Task<AdminSkuListResponse> GetSkusAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.ProductsWrite);
        var (normalizedPage, normalizedPageSize) = NormalizePaging(page, pageSize);

        return _adminCommerceRepository.GetSkusAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken);
    }

    public Task<AdminInventoryListResponse> GetInventoryAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.InventoryRead);
        var (normalizedPage, normalizedPageSize) = NormalizePaging(page, pageSize);

        return _adminCommerceRepository.GetInventoryAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken);
    }

    public Task<AdminOrderListResponse> GetOrdersAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.OrdersRead);
        var (normalizedPage, normalizedPageSize) = NormalizePaging(page, pageSize);

        return _adminCommerceRepository.GetOrdersAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken);
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

        return (normalizedPage, normalizedPageSize);
    }

    private static AdminProductUpsertRequest NormalizeProductRequest(AdminProductUpsertRequest request)
    {
        if (request is null)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_REQUEST_REQUIRED", "請提供商品資料。");
        }

        if (request.CategoryId <= 0)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_CATEGORY_REQUIRED", "請選擇商品分類。");
        }

        var productName = NormalizeRequired(request.ProductName, 200, "ADMIN_PRODUCT_NAME_REQUIRED", "商品名稱為必填。");
        var status = NormalizeRequired(request.Status, 30, "ADMIN_PRODUCT_STATUS_REQUIRED", "請選擇商品狀態。");

        // 商品狀態使用白名單，避免前端任意送入資料庫未定義狀態造成流程分歧。
        if (!ValidProductStatuses.Contains(status))
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_STATUS_INVALID", "商品狀態不符合系統允許值。");
        }

        if (request.Sku is null)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_SKU_REQUIRED", "至少需要設定一組商品規格與價格。");
        }

        var sku = NormalizeSkuRequest(request.Sku, productName);
        var images = NormalizeImages(request.MainImageUrl, request.Images);

        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            // 上架前先在 Service 層確認最小可販售條件，Repository 不負責決定商品是否可上架。
            if (!string.Equals(sku.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                throw new AdminSystemValidationException("ADMIN_PRODUCT_ACTIVE_SKU_REQUIRED", "商品上架前至少需要一組啟用中的 SKU。");
            }

            if (images.Count == 0)
            {
                throw new AdminSystemValidationException("ADMIN_PRODUCT_ACTIVE_IMAGE_REQUIRED", "商品上架前至少需要一張商品圖片。");
            }
        }

        return new AdminProductUpsertRequest(
            request.CategoryId,
            productName,
            NormalizeOptional(request.BrandName, 100),
            NormalizeOptional(request.ShortDescription, 500),
            NormalizeOptional(request.FullDescription, 4000),
            status,
            request.IsFeatured,
            sku,
            images.FirstOrDefault(image => image.IsMainImage)?.ImageUrl,
            images,
            NormalizeOptional(request.Reason, 500));
    }

    private static AdminProductSkuUpsertRequest NormalizeSkuRequest(
        AdminProductSkuUpsertRequest sku,
        string productName)
    {
        var skuName = string.IsNullOrWhiteSpace(sku.SkuName)
            ? productName
            : NormalizeRequired(sku.SkuName, 200, "ADMIN_PRODUCT_SKU_NAME_REQUIRED", "SKU 名稱為必填。");
        var status = NormalizeRequired(sku.Status, 30, "ADMIN_PRODUCT_SKU_STATUS_REQUIRED", "請選擇 SKU 狀態。");

        if (!ValidSkuStatuses.Contains(status))
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_SKU_STATUS_INVALID", "SKU 狀態不符合系統允許值。");
        }

        if (sku.ListPrice < 0 || sku.SalePrice < 0 || sku.BasePrice < 0)
        {
            throw new AdminSystemValidationException("ADMIN_PRODUCT_PRICE_INVALID", "商品價格不可小於 0。");
        }

        if (sku.SalePrice > sku.ListPrice)
        {
            // 售價規則在後端檢查，避免前端表單驗證被繞過後寫入不合理價格。
            throw new AdminSystemValidationException("ADMIN_PRODUCT_SALE_PRICE_INVALID", "售價不可高於原價。");
        }

        var size = NormalizeOptional(sku.Size, 200);
        var color = NormalizeOptional(sku.Color, 200);
        var capacity = NormalizeOptional(sku.Capacity, 200);
        var specText = NormalizeOptional(sku.SpecText, 500)
            ?? BuildSpecText(size, color, capacity);

        return new AdminProductSkuUpsertRequest(
            sku.SkuId,
            NormalizeOptional(sku.SkuNo, 50),
            skuName,
            specText,
            sku.ListPrice,
            sku.SalePrice,
            sku.BasePrice,
            size,
            color,
            capacity,
            status);
    }

    private static IReadOnlyCollection<AdminProductImageUpsertRequest> NormalizeImages(
        string? mainImageUrl,
        IReadOnlyCollection<AdminProductImageUpsertRequest>? images)
    {
        // 以 URL 作為去重鍵，避免同一張圖重複寫入並造成主圖排序混亂。
        var normalizedByUrl = new Dictionary<string, AdminProductImageUpsertRequest>(StringComparer.OrdinalIgnoreCase);
        var mainUrl = NormalizeImageUrl(mainImageUrl, allowEmpty: true);

        if (images is not null)
        {
            foreach (var image in images)
            {
                var imageUrl = NormalizeImageUrl(image.ImageUrl, allowEmpty: true);
                if (imageUrl is null)
                {
                    continue;
                }

                normalizedByUrl[imageUrl] = new AdminProductImageUpsertRequest(
                    image.ImageId,
                    imageUrl,
                    NormalizeOptional(image.AltText, 200),
                    image.IsMainImage,
                    Math.Max(0, image.SortOrder));
            }
        }

        if (mainUrl is not null)
        {
            normalizedByUrl[mainUrl] = new AdminProductImageUpsertRequest(
                normalizedByUrl.TryGetValue(mainUrl, out var existingImage) ? existingImage.ImageId : null,
                mainUrl,
                normalizedByUrl.TryGetValue(mainUrl, out existingImage) ? existingImage.AltText : null,
                true,
                normalizedByUrl.TryGetValue(mainUrl, out existingImage) ? existingImage.SortOrder : 0);
        }

        var orderedImages = normalizedByUrl.Values
            .OrderBy(image => image.SortOrder)
            .ThenBy(image => image.ImageUrl, StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToArray();

        if (orderedImages.Length == 0)
        {
            return [];
        }

        var mainImage = orderedImages.FirstOrDefault(image => image.IsMainImage)
            ?? orderedImages[0];

        return orderedImages
            .Select((image, index) => image with
            {
                IsMainImage = string.Equals(image.ImageUrl, mainImage.ImageUrl, StringComparison.OrdinalIgnoreCase),
                SortOrder = index
            })
            .ToArray();
    }

    private static string? NormalizeImageUrl(string? value, bool allowEmpty)
    {
        var normalized = NormalizeOptional(value, 1000);
        if (normalized is null)
        {
            if (allowEmpty)
            {
                return null;
            }

            throw new AdminSystemValidationException("ADMIN_PRODUCT_IMAGE_REQUIRED", "請提供商品圖片路徑。");
        }

        if (normalized.Contains('\\', StringComparison.Ordinal)
            || normalized.Contains("..", StringComparison.Ordinal))
        {
            // 阻擋路徑穿越與 Windows 反斜線，避免後續圖片上傳/檔案服務擴充時誤用危險路徑。
            throw new AdminSystemValidationException("ADMIN_PRODUCT_IMAGE_PATH_INVALID", "商品圖片路徑不可包含路徑穿越字元。");
        }

        string path;
        if (Uri.TryCreate(normalized, UriKind.Absolute, out var absoluteUri))
        {
            if (absoluteUri.Scheme != Uri.UriSchemeHttp && absoluteUri.Scheme != Uri.UriSchemeHttps)
            {
                throw new AdminSystemValidationException("ADMIN_PRODUCT_IMAGE_SCHEME_INVALID", "商品圖片只允許 http 或 https 網址。");
            }

            path = absoluteUri.AbsolutePath;
        }
        else
        {
            if (!normalized.StartsWith("/", StringComparison.Ordinal) || normalized.StartsWith("//", StringComparison.Ordinal))
            {
                throw new AdminSystemValidationException("ADMIN_PRODUCT_IMAGE_PATH_INVALID", "站內商品圖片路徑必須以 / 開頭。");
            }

            path = normalized.Split('?', '#')[0];
        }

        var extension = Path.GetExtension(path);
        if (!AllowedImageExtensions.Contains(extension))
        {
            // 先在 Service 限制圖片副檔名；正式上傳仍需搭配 MIME 檢查、大小限制與儲存隔離。
            throw new AdminSystemValidationException("ADMIN_PRODUCT_IMAGE_EXTENSION_INVALID", "商品圖片僅允許 jpg、jpeg、png、webp。");
        }

        return normalized;
    }

    private static string? BuildSpecText(params string?[] values)
    {
        // 未輸入完整規格文字時，以尺寸/顏色/容量合成可讀描述，維持 SKU 顯示一致性。
        var specParts = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .ToArray();

        return specParts.Length == 0 ? null : string.Join(" / ", specParts);
    }

    private static string NormalizeRequired(string value, int maxLength, string code, string message)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new AdminSystemValidationException(code, message);
        }

        if (normalized.Length > maxLength)
        {
            throw new AdminSystemValidationException("ADMIN_FIELD_TOO_LONG", $"欄位長度不可超過 {maxLength} 個字。");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new AdminSystemValidationException("ADMIN_FIELD_TOO_LONG", $"欄位長度不可超過 {maxLength} 個字。");
        }

        return normalized;
    }

    private static long GetActorUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdText, out var userId) || userId <= 0)
        {
            throw new AdminSystemValidationException("ADMIN_ACTOR_INVALID", "無法辨識目前後台操作者。");
        }

        return userId;
    }

    private static void EnsurePermission(ClaimsPrincipal user, string permission)
    {
        var hasPermission = user.Claims.Any(claim =>
            string.Equals(claim.Type, AdminClaimTypes.Permission, StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException($"ADMIN_PERMISSION_REQUIRED:{permission}");
        }
    }
}
