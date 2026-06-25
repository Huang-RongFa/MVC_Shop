using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.DTOs.Storefront.Cart;
using MyWeb.Application.DTOs.Storefront.Products;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.Commerce;
using MyWeb.Application.Interfaces.Admin.System;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Application.Security;
using MyWeb.Application.Services.Admin.Commerce;
using MyWeb.Application.Services.Admin.System;
using MyWeb.Application.Services.Cart;
using MyWeb.Application.Services.Pages;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Orders;
using MyWeb.Models.ViewModels.Storefront;

var tests = new (string Name, Func<Task> Run)[]
{
    ("Admin system users require Users.Manage permission", AdminSystemServiceTests.UsersRequireUsersManagePermission),
    ("Admin system roles require Roles.Manage permission", AdminSystemServiceTests.RolesRequireRolesManagePermission),
    ("Admin system audit trail requires AuditLogs.Read permission", AdminSystemServiceTests.AuditTrailRequiresAuditLogsReadPermission),
    ("Admin system settings require Settings.Manage permission", AdminSystemServiceTests.SettingsRequireSettingsManagePermission),
    ("Admin commerce products require Products.Read permission", AdminSystemServiceTests.CommerceProductsRequireProductsReadPermission),
    ("Admin commerce skus require Products.Write permission", AdminSystemServiceTests.CommerceSkusRequireProductsWritePermission),
    ("Admin commerce inventory requires Inventory.Read permission", AdminSystemServiceTests.CommerceInventoryRequiresInventoryReadPermission),
    ("Admin commerce orders require Orders.Read permission", AdminSystemServiceTests.CommerceOrdersRequireOrdersReadPermission),
    ("Admin commerce page size falls back to 10", AdminSystemServiceTests.CommercePageSizeFallsBackToTen),
    ("Admin sku DTO does not expose cost price", AdminSystemServiceTests.CommerceSkuDtoDoesNotExposeCostPrice),
    ("Admin order DTO does not expose receiver details", AdminSystemServiceTests.CommerceOrderDtoDoesNotExposeReceiverDetails),
    ("Admin system audit trail calls repository when authorized", AdminSystemServiceTests.AuditTrailCallsRepositoryWhenAuthorized),
    ("Admin users page size falls back to 10", AdminSystemServiceTests.UsersPageSizeFallsBackToTen),
    ("Admin user status update requires Users.Manage permission", AdminSystemServiceTests.UpdateUserStatusRequiresUsersManagePermission),
    ("Admin cannot deactivate own account", AdminSystemServiceTests.AdminCannotDeactivateOwnAccount),
    ("Admin role permission update requires Roles.Manage permission", AdminSystemServiceTests.UpdateRolePermissionsRequiresRolesManagePermission),
    ("Admin role permission update requires permissions", AdminSystemServiceTests.UpdateRolePermissionsRequiresPermissions),
    ("Admin user DTO does not expose sensitive fields", AdminSystemServiceTests.UserDtoDoesNotExposeSensitiveFields),
    ("Admin audit DTOs do not expose raw sensitive payloads", AdminSystemServiceTests.AuditDtosDoNotExposeRawSensitivePayloads),
    ("Admin system settings describe security controls", AdminSystemServiceTests.SettingsDescribeSecurityControls),
    ("Storefront keeps admin session without customer cart", StorefrontAuthBoundaryTests.AdminCanBrowseStorefrontWithoutCustomerCart),
    ("Cart mutations require customer identity", StorefrontAuthBoundaryTests.CartMutationsRequireCustomerIdentity)
};

var failed = 0;
foreach (var test in tests)
{
    try
    {
        await test.Run();
        Console.WriteLine($"[PASS] {test.Name}");
    }
    catch (Exception ex)
    {
        failed++;
        Console.Error.WriteLine($"[FAIL] {test.Name}");
        Console.Error.WriteLine(ex);
    }
}

if (failed > 0)
{
    Environment.Exit(1);
}

static class AdminSystemServiceTests
{
    public static async Task UsersRequireUsersManagePermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetUsersAsync(new ClaimsPrincipal(new ClaimsIdentity()), 1, 10, CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Users.Manage", ex.Message);
    }

    public static async Task RolesRequireRolesManagePermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetRolesAsync(new ClaimsPrincipal(new ClaimsIdentity()), CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Roles.Manage", ex.Message);
    }

    public static async Task AuditTrailRequiresAuditLogsReadPermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetAuditTrailAsync(new ClaimsPrincipal(new ClaimsIdentity()), CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:AuditLogs.Read", ex.Message);
    }

    public static async Task SettingsRequireSettingsManagePermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetSettingsAsync(new ClaimsPrincipal(new ClaimsIdentity()), CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Settings.Manage", ex.Message);
    }

    public static async Task CommerceProductsRequireProductsReadPermission()
    {
        var service = new AdminCommerceService(new FakeAdminCommerceRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetProductsAsync(new ClaimsPrincipal(new ClaimsIdentity()), 1, 10, CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Products.Read", ex.Message);
    }

    public static async Task CommerceSkusRequireProductsWritePermission()
    {
        var service = new AdminCommerceService(new FakeAdminCommerceRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetSkusAsync(new ClaimsPrincipal(new ClaimsIdentity()), 1, 10, CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Products.Write", ex.Message);
    }

    public static async Task CommerceInventoryRequiresInventoryReadPermission()
    {
        var service = new AdminCommerceService(new FakeAdminCommerceRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetInventoryAsync(new ClaimsPrincipal(new ClaimsIdentity()), 1, 10, CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Inventory.Read", ex.Message);
    }

    public static async Task CommerceOrdersRequireOrdersReadPermission()
    {
        var service = new AdminCommerceService(new FakeAdminCommerceRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.GetOrdersAsync(new ClaimsPrincipal(new ClaimsIdentity()), 1, 10, CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Orders.Read", ex.Message);
    }

    public static async Task CommercePageSizeFallsBackToTen()
    {
        var repository = new FakeAdminCommerceRepository();
        var service = new AdminCommerceService(repository);

        await service.GetInventoryAsync(
            PrincipalWithPermission(AdminPermissionCodes.InventoryRead),
            page: -7,
            pageSize: 999,
            CancellationToken.None);

        AssertEqual(1, repository.LastRequestedPage);
        AssertEqual(10, repository.LastRequestedPageSize);
    }

    public static Task CommerceSkuDtoDoesNotExposeCostPrice()
    {
        var propertyNames = typeof(AdminSkuListItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AssertFalse(propertyNames.Contains("CostPrice"), "Admin SKU list DTO must not expose CostPrice.");

        return Task.CompletedTask;
    }

    public static Task CommerceOrderDtoDoesNotExposeReceiverDetails()
    {
        var propertyNames = typeof(AdminOrderListItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AssertFalse(propertyNames.Contains("ReceiverName"), "Admin order list DTO must not expose ReceiverName.");
        AssertFalse(propertyNames.Contains("ReceiverPhone"), "Admin order list DTO must not expose ReceiverPhone.");
        AssertFalse(propertyNames.Contains("ReceiverAddress"), "Admin order list DTO must not expose ReceiverAddress.");
        AssertFalse(propertyNames.Contains("Remark"), "Admin order list DTO must not expose order remarks in the list.");

        return Task.CompletedTask;
    }

    public static async Task AuditTrailCallsRepositoryWhenAuthorized()
    {
        var repository = new FakeAdminSystemRepository();
        var service = new AdminSystemService(repository);

        await service.GetAuditTrailAsync(
            PrincipalWithPermission(AdminPermissionCodes.AuditLogsRead),
            CancellationToken.None);

        AssertTrue(repository.AuditTrailRequested, "Audit trail repository should be called after permission passes.");
    }

    public static async Task UsersPageSizeFallsBackToTen()
    {
        var repository = new FakeAdminSystemRepository();
        var service = new AdminSystemService(repository);

        await service.GetUsersAsync(
            PrincipalWithPermission(AdminPermissionCodes.UsersManage),
            page: -5,
            pageSize: 999,
            CancellationToken.None);

        AssertEqual(1, repository.LastRequestedPage);
        AssertEqual(10, repository.LastRequestedPageSize);
    }

    public static async Task UpdateUserStatusRequiresUsersManagePermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateUserStatusAsync(
                new ClaimsPrincipal(new ClaimsIdentity()),
                10,
                new AdminUpdateUserStatusRequest("Inactive", "測試停用"),
                new AdminSystemRequestContext(null, null),
                CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Users.Manage", ex.Message);
    }

    public static async Task AdminCannotDeactivateOwnAccount()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<AdminSystemValidationException>(
            () => service.UpdateUserStatusAsync(
                PrincipalWithPermission(AdminPermissionCodes.UsersManage, userId: 7),
                7,
                new AdminUpdateUserStatusRequest("Inactive", "測試停用"),
                new AdminSystemRequestContext(null, null),
                CancellationToken.None));

        AssertEqual("ADMIN_CANNOT_DISABLE_SELF", ex.Code);
    }

    public static async Task UpdateRolePermissionsRequiresRolesManagePermission()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<UnauthorizedAccessException>(
            () => service.UpdateRolePermissionsAsync(
                new ClaimsPrincipal(new ClaimsIdentity()),
                5,
                new AdminUpdateRolePermissionsRequest(["Orders.Read"], "調整權限"),
                new AdminSystemRequestContext(null, null),
                CancellationToken.None));

        AssertEqual("ADMIN_PERMISSION_REQUIRED:Roles.Manage", ex.Message);
    }

    public static async Task UpdateRolePermissionsRequiresPermissions()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var ex = await AssertThrowsAsync<AdminSystemValidationException>(
            () => service.UpdateRolePermissionsAsync(
                PrincipalWithPermission(AdminPermissionCodes.RolesManage, userId: 7),
                5,
                new AdminUpdateRolePermissionsRequest([], "清空權限"),
                new AdminSystemRequestContext(null, null),
                CancellationToken.None));

        AssertEqual("ADMIN_ROLE_PERMISSION_REQUIRED", ex.Code);
    }

    public static Task UserDtoDoesNotExposeSensitiveFields()
    {
        var propertyNames = typeof(AdminUserListItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AssertFalse(propertyNames.Contains("PasswordHash"), "Admin user DTO must not expose PasswordHash.");
        AssertFalse(propertyNames.Contains("PasswordSalt"), "Admin user DTO must not expose PasswordSalt.");
        AssertFalse(propertyNames.Contains("SecurityStamp"), "Admin user DTO must not expose SecurityStamp.");

        return Task.CompletedTask;
    }

    public static Task AuditDtosDoNotExposeRawSensitivePayloads()
    {
        var dataChangeProperties = typeof(AdminDataChangeLogItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var systemErrorProperties = typeof(AdminSystemErrorLogItemDto)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        AssertFalse(dataChangeProperties.Contains("OldValueJson"), "Audit list DTO must not expose raw old JSON payload.");
        AssertFalse(dataChangeProperties.Contains("NewValueJson"), "Audit list DTO must not expose raw new JSON payload.");
        AssertFalse(systemErrorProperties.Contains("StackTrace"), "System error list DTO must not expose StackTrace.");
        AssertFalse(systemErrorProperties.Contains("RequestBody"), "System error list DTO must not expose RequestBody.");

        return Task.CompletedTask;
    }

    public static async Task SettingsDescribeSecurityControls()
    {
        var service = new AdminSystemService(new FakeAdminSystemRepository());

        var response = await service.GetSettingsAsync(
            PrincipalWithPermission(AdminPermissionCodes.SettingsManage),
            CancellationToken.None);

        AssertTrue(response.Sections.Any(section => section.SectionKey == "security"), "Security section should exist.");
        AssertTrue(
            response.Sections
                .SelectMany(section => section.Items)
                .Any(item => item.Key == "csrf" && item.IsImplemented),
            "CSRF setting should be marked as implemented.");
    }

    private static async Task<TException> AssertThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException ex)
        {
            return ex;
        }

        throw new InvalidOperationException($"Expected exception {typeof(TException).Name} was not thrown.");
    }

    private static void AssertEqual<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertFalse(bool condition, string message)
    {
        AssertTrue(!condition, message);
    }

    private static ClaimsPrincipal PrincipalWithPermission(string permission, long userId = 1)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(AdminClaimTypes.Permission, permission)
        ]);

        return new ClaimsPrincipal(identity);
    }

    private sealed class FakeAdminSystemRepository : IAdminSystemRepository
    {
        public int LastRequestedPage { get; private set; }

        public int LastRequestedPageSize { get; private set; }

        public bool AuditTrailRequested { get; private set; }

        public Task<AdminUserListResponse> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken)
        {
            LastRequestedPage = page;
            LastRequestedPageSize = pageSize;

            return Task.FromResult(new AdminUserListResponse(
                0,
                page,
                pageSize,
                1,
                []));
        }

        public Task<AdminRolePermissionMatrixResponse> GetRolesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AdminAuditTrailResponse> GetAuditTrailAsync(CancellationToken cancellationToken)
        {
            AuditTrailRequested = true;
            return Task.FromResult(new AdminAuditTrailResponse([], [], []));
        }

        public Task<AdminMutationResponse> UpdateUserStatusAsync(
            long actorUserId,
            long targetUserId,
            string status,
            string? reason,
            AdminSystemRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminMutationResponse("updated"));
        }

        public Task<AdminMutationResponse> UpdateUserRolesAsync(
            long actorUserId,
            long targetUserId,
            IReadOnlyCollection<string> roleCodes,
            string? reason,
            AdminSystemRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminMutationResponse("updated"));
        }

        public Task<AdminMutationResponse> UpdateRolePermissionsAsync(
            long actorUserId,
            int roleId,
            IReadOnlyCollection<string> permissionCodes,
            string? reason,
            AdminSystemRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminMutationResponse("updated"));
        }
    }

    private sealed class FakeAdminCommerceRepository : IAdminCommerceRepository
    {
        public int LastRequestedPage { get; private set; }

        public int LastRequestedPageSize { get; private set; }

        public Task<AdminProductListResponse> GetProductsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            CapturePaging(page, pageSize);

            return Task.FromResult(new AdminProductListResponse(
                0,
                page,
                pageSize,
                1,
                [],
                [],
                []));
        }

        public Task<AdminProductEditResponse> GetProductCreateOptionsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminProductEditResponse(
                null,
                null,
                null,
                string.Empty,
                null,
                null,
                null,
                "Draft",
                false,
                new AdminProductSkuEditDto(null, null, string.Empty, null, 0m, 0m, null, null, null, null, "Inactive"),
                [],
                ["Draft", "Active", "Inactive", "Archived"],
                ["Active", "Inactive"],
                []));
        }

        public Task<AdminProductEditResponse> GetProductForEditAsync(long productId, CancellationToken cancellationToken)
        {
            return GetProductCreateOptionsAsync(cancellationToken);
        }

        public Task<AdminProductMutationResponse> CreateProductAsync(
            long actorUserId,
            AdminProductUpsertRequest request,
            AdminSystemRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminProductMutationResponse("created", 1));
        }

        public Task<AdminProductMutationResponse> UpdateProductAsync(
            long actorUserId,
            long productId,
            AdminProductUpsertRequest request,
            AdminSystemRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new AdminProductMutationResponse("updated", productId));
        }

        public Task<AdminSkuListResponse> GetSkusAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            CapturePaging(page, pageSize);

            return Task.FromResult(new AdminSkuListResponse(
                0,
                page,
                pageSize,
                1,
                [],
                []));
        }

        public Task<AdminInventoryListResponse> GetInventoryAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            CapturePaging(page, pageSize);

            return Task.FromResult(new AdminInventoryListResponse(
                0,
                page,
                pageSize,
                1,
                [],
                []));
        }

        public Task<AdminOrderListResponse> GetOrdersAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            CapturePaging(page, pageSize);

            return Task.FromResult(new AdminOrderListResponse(
                0,
                page,
                pageSize,
                1,
                [],
                [],
                [],
                []));
        }

        private void CapturePaging(int page, int pageSize)
        {
            LastRequestedPage = page;
            LastRequestedPageSize = pageSize;
        }
    }
}

static class StorefrontAuthBoundaryTests
{
    public static async Task AdminCanBrowseStorefrontWithoutCustomerCart()
    {
        var cartService = new FakeCartService();
        var catalogService = new FakeProductCatalogQueryService();
        var pageService = new StorefrontPageService(cartService, catalogService);

        var response = await pageService.GetHomeAsync(
            PrincipalWithUserType("Admin"),
            CancellationToken.None);

        AssertTrue(response.IsAuthenticated, "Admin session should stay authenticated on storefront pages.");
        AssertEqual(0, response.CartItemCount);
        AssertEqual(0, cartService.GetCartCallCount);
        AssertFalse(
            response.FeaturedProducts.Any(product => product.CanAddToCart),
            "Admin/Staff storefront preview must not expose customer cart actions.");
    }

    public static async Task CartMutationsRequireCustomerIdentity()
    {
        var repository = new FakeCartRepository();
        var service = new CartService(repository);

        var result = await service.AddItemAsync(
            PrincipalWithUserType("Admin"),
            new AddCartItemRequest
            {
                ProductId = 101,
                Quantity = 1
            },
            CancellationToken.None);

        AssertFalse(result.Succeeded, "Admin/Staff should not be allowed to mutate a customer cart.");
        AssertFalse(
            repository.DefaultSkuRequested,
            "CartService should reject non-customer identities before querying product/cart data.");
    }

    private static ClaimsPrincipal PrincipalWithUserType(string userType)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Name, "管理員"),
            new Claim(AdminClaimTypes.UserType, userType)
        ], "Test");

        return new ClaimsPrincipal(identity);
    }

    private static void AssertEqual<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void AssertFalse(bool condition, string message)
    {
        AssertTrue(!condition, message);
    }

    private sealed class FakeCartService : ICartService
    {
        public int GetCartCallCount { get; private set; }

        public Task<CartOperationResult> AddItemAsync(
            ClaimsPrincipal user,
            AddCartItemRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CartOperationResult.Failure("not implemented"));
        }

        public Task<CartOperationResult> UpdateQuantityAsync(
            ClaimsPrincipal user,
            long cartItemId,
            int quantity,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CartOperationResult.Failure("not implemented"));
        }

        public Task<CartOperationResult> RemoveItemAsync(
            ClaimsPrincipal user,
            long cartItemId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CartOperationResult.Failure("not implemented"));
        }

        public Task<CartViewModel> GetCartAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            GetCartCallCount++;
            return Task.FromResult(new CartViewModel
            {
                Items =
                [
                    new CartItemViewModel
                    {
                        ProductId = 101,
                        Quantity = 2
                    }
                ],
                SubtotalText = "NT$ 200",
                DiscountTotalText = "NT$ 0",
                EstimatedTotalText = "NT$ 200",
                CanCheckout = true
            });
        }

        public Task<CartCouponPreviewResult> PreviewCouponAsync(
            ClaimsPrincipal user,
            string? couponCode,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(CartCouponPreviewResult.Create(
                false,
                "not implemented",
                string.Empty,
                string.Empty,
                "NT$ 0",
                "NT$ 0",
                "NT$ 0",
                false));
        }
    }

    private sealed class FakeProductCatalogQueryService : IProductCatalogQueryService
    {
        public Task<IReadOnlyList<CategoryCardViewModel>> GetCategoriesAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<CategoryCardViewModel>>([]);
        }

        public Task<IReadOnlyList<ProductCardViewModel>> GetFeaturedProductsAsync(
            int take,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyList<ProductCardViewModel>>(
            [
                new ProductCardViewModel
                {
                    ProductId = 101,
                    Name = "測試商品",
                    PriceText = "NT$ 100",
                    AvailableStockQuantity = 5
                }
            ]);
        }

        public Task<ProductCatalogPageResult> SearchProductsAsync(
            ProductCatalogSearchCriteria criteria,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ProductCatalogPageResult(0, [], [], []));
        }

        public Task<ProductDetailViewModel?> GetProductDetailAsync(
            long productId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ProductDetailViewModel?>(null);
        }
    }

    private sealed class FakeCartRepository : ICartRepository
    {
        public bool DefaultSkuRequested { get; private set; }

        public Task<ProductSku?> GetDefaultPurchasableSkuAsync(
            long productId,
            CancellationToken cancellationToken)
        {
            DefaultSkuRequested = true;
            return Task.FromResult<ProductSku?>(null);
        }

        public Task<ShoppingCart?> GetActiveCartForUpdateAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cart lookup should not be called for non-customer identities.");
        }

        public Task<ShoppingCart?> GetActiveCartForReadAsync(
            long userId,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cart lookup should not be called for non-customer identities.");
        }

        public Task<ShoppingCartItem?> GetCartItemForUpdateAsync(
            long userId,
            long cartItemId,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Cart item lookup should not be called for non-customer identities.");
        }

        public Task<int> GetAvailableQuantityAsync(
            long skuId,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Inventory lookup should not be called for non-customer identities.");
        }

        public void AddCart(ShoppingCart cart)
        {
            throw new InvalidOperationException("Cart creation should not be called for non-customer identities.");
        }

        public void RemoveCartItem(ShoppingCartItem item)
        {
            throw new InvalidOperationException("Cart removal should not be called for non-customer identities.");
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Save should not be called for non-customer identities.");
        }
    }
}
