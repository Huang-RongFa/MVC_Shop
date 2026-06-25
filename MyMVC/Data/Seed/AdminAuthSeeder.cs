using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Data.Seed;

public sealed class AdminAuthSeeder
{
    private const string DefaultPassword = "Fraser_Love_Yun";
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ILogger<AdminAuthSeeder> _logger;

    public AdminAuthSeeder(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        ILogger<AdminAuthSeeder> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;

            await EnsurePermissionsAsync(now, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var permissionCodes = AdminPermissionSeeds.Select(seed => seed.Code).ToArray();
            var permissionsByCode = await _dbContext.Permissions
                .Where(permission => permissionCodes.Contains(permission.PermissionCode))
                .ToDictionaryAsync(permission => permission.PermissionCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

            await EnsureRolesAsync(now, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var roleCodes = AdminRoleSeeds.Select(seed => seed.Code).ToArray();
            var rolesByCode = await _dbContext.Roles
                .Where(role => roleCodes.Contains(role.RoleCode))
                .ToDictionaryAsync(role => role.RoleCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

            await EnsureRolePermissionsAsync(rolesByCode, permissionsByCode, now, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await EnsureUsersAsync(rolesByCode, now, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });

        _logger.LogInformation("Admin auth seed completed. Accounts: {AccountCount}", AdminUserSeeds.Length);
    }

    private async Task EnsurePermissionsAsync(DateTime now, CancellationToken cancellationToken)
    {
        var permissionCodes = AdminPermissionSeeds.Select(seed => seed.Code).ToArray();
        var existingPermissions = await _dbContext.Permissions
            .Where(permission => permissionCodes.Contains(permission.PermissionCode))
            .ToDictionaryAsync(permission => permission.PermissionCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seed in AdminPermissionSeeds)
        {
            if (existingPermissions.TryGetValue(seed.Code, out var permission))
            {
                permission.PermissionName = seed.Name;
                permission.ModuleName = seed.ModuleName;
                permission.Description = seed.Description;
                continue;
            }

            _dbContext.Permissions.Add(new Permission
            {
                PermissionCode = seed.Code,
                PermissionName = seed.Name,
                ModuleName = seed.ModuleName,
                Description = seed.Description,
                CreatedAt = now
            });
        }
    }

    private async Task EnsureRolesAsync(DateTime now, CancellationToken cancellationToken)
    {
        var roleCodes = AdminRoleSeeds.Select(seed => seed.Code).ToArray();
        var existingRoles = await _dbContext.Roles
            .Where(role => roleCodes.Contains(role.RoleCode))
            .ToDictionaryAsync(role => role.RoleCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seed in AdminRoleSeeds)
        {
            if (existingRoles.TryGetValue(seed.Code, out var role))
            {
                role.RoleName = seed.Name;
                role.Description = seed.Description;
                role.IsSystemRole = true;
                role.IsDeleted = false;
                continue;
            }

            _dbContext.Roles.Add(new Role
            {
                RoleCode = seed.Code,
                RoleName = seed.Name,
                Description = seed.Description,
                IsSystemRole = true,
                CreatedAt = now,
                IsDeleted = false
            });
        }
    }

    private async Task EnsureRolePermissionsAsync(
        IReadOnlyDictionary<string, Role> rolesByCode,
        IReadOnlyDictionary<string, Permission> permissionsByCode,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var roleIds = rolesByCode.Values.Select(role => role.RoleId).ToArray();
        var permissionIds = permissionsByCode.Values.Select(permission => permission.PermissionId).ToArray();
        var existingKeys = (await _dbContext.RolePermissions
                .Where(rolePermission => roleIds.Contains(rolePermission.RoleId)
                    && permissionIds.Contains(rolePermission.PermissionId))
                .Select(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
                .ToListAsync(cancellationToken))
            .Select(item => $"{item.RoleId}:{item.PermissionId}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var roleSeed in AdminRoleSeeds)
        {
            var role = rolesByCode[roleSeed.Code];

            foreach (var permissionCode in roleSeed.PermissionCodes)
            {
                var permission = permissionsByCode[permissionCode];
                var key = $"{role.RoleId}:{permission.PermissionId}";
                if (existingKeys.Contains(key))
                {
                    continue;
                }

                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.RoleId,
                    PermissionId = permission.PermissionId,
                    CreatedAt = now
                });
                existingKeys.Add(key);
            }
        }
    }

    private async Task EnsureUsersAsync(
        IReadOnlyDictionary<string, Role> rolesByCode,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var accounts = AdminUserSeeds.Select(seed => seed.Account).ToArray();
        var existingUsers = await _dbContext.Users
            .Include(user => user.UserRoles)
            .Where(user => accounts.Contains(user.Account))
            .ToDictionaryAsync(user => user.Account, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seed in AdminUserSeeds)
        {
            if (!rolesByCode.TryGetValue(seed.RoleCode, out var role))
            {
                throw new InvalidOperationException($"Seed role '{seed.RoleCode}' was not created.");
            }

            if (!existingUsers.TryGetValue(seed.Account, out var user))
            {
                user = new User
                {
                    UserNo = seed.UserNo,
                    Account = seed.Account,
                    CreatedAt = now
                };
                _dbContext.Users.Add(user);
            }

            user.UserNo = seed.UserNo;
            user.Email = seed.Email;
            user.DisplayName = seed.DisplayName;
            user.UserType = seed.UserType;
            user.Status = "Active";
            user.PasswordSalt = null;
            user.PasswordHash = _passwordHasher.HashPassword(user, DefaultPassword);
            user.UpdatedAt = now;
            user.IsDeleted = false;

            var hasRole = user.UserRoles.Any(userRole => userRole.RoleId == role.RoleId);
            if (hasRole)
            {
                continue;
            }

            user.UserRoles.Add(new UserRole
            {
                User = user,
                RoleId = role.RoleId,
                AssignedAt = now
            });
        }
    }

    private static readonly PermissionSeed[] AdminPermissionSeeds =
    [
        new("Dashboard.Read", "檢視營運總覽", "Dashboard", "檢視後台首頁與個人化管理入口。"),
        new("Analytics.Read", "檢視數據分析", "Analytics", "檢視營收、訂單、退款率與會員成長等營運指標。"),
        new("Users.Manage", "管理使用者", "Accounts", "管理前台會員、後台員工與帳號狀態。"),
        new("Roles.Manage", "管理角色權限", "Accounts", "管理角色、權限與角色授權。"),
        new("Products.Read", "檢視商品", "Catalog", "查詢商品、分類、圖片與 SKU。"),
        new("Products.Write", "維護商品", "Catalog", "新增、修改、上下架商品與 SKU。"),
        new("Inventory.Read", "檢視庫存", "Inventory", "查詢庫存、保留量與安全庫存。"),
        new("Inventory.Adjust", "調整庫存", "Inventory", "執行庫存調整並保留稽核紀錄。"),
        new("Orders.Read", "檢視訂單", "Orders", "查詢訂單主檔、明細與狀態歷程。"),
        new("Orders.Write", "維護訂單", "Orders", "更新訂單狀態與營運處理備註。"),
        new("Payments.Read", "檢視付款", "Payments", "查詢付款單與金流交易狀態。"),
        new("Shipments.Manage", "管理出貨", "Shipments", "建立出貨、更新物流與出貨狀態。"),
        new("Refunds.Manage", "管理退款", "Refunds", "處理整筆與部分退款流程。"),
        new("Coupons.Manage", "管理優惠券", "Promotions", "建立與維護優惠券規則。"),
        new("Promotions.Manage", "管理促銷", "Promotions", "建立與維護促銷活動及商品範圍。"),
        new("Messages.Read", "檢視訊息通知", "Messages", "查看客服訊息、系統通知與營運警示。"),
        new("AuditLogs.Read", "檢視稽核紀錄", "Audit", "查詢後台操作紀錄與資料異動紀錄。"),
        new("Settings.Manage", "管理系統設定", "Settings", "管理後台偏好、通知與安全設定。")
    ];

    private static readonly RoleSeed[] AdminRoleSeeds =
    [
        new("SUPER_ADMIN", "系統管理員", "擁有所有後台管理頁面與系統設定權限。", PermissionCodes.All),
        new("OPERATIONS_MANAGER", "營運主管", "可查看營運指標並協調商品、訂單、出貨、退款與行銷作業。",
        [
            "Dashboard.Read", "Analytics.Read", "Products.Read", "Inventory.Read", "Orders.Read", "Orders.Write",
            "Payments.Read", "Shipments.Manage", "Refunds.Manage", "Coupons.Manage", "Promotions.Manage",
            "Messages.Read", "AuditLogs.Read"
        ]),
        new("PRODUCT_MANAGER", "商品管理員", "負責商品主檔、SKU、上下架與商品資料品質。",
        [
            "Dashboard.Read", "Products.Read", "Products.Write", "Inventory.Read", "Analytics.Read"
        ]),
        new("ORDER_MANAGER", "訂單管理員", "負責訂單查詢、狀態處理、付款對帳、出貨協調與退款初審。",
        [
            "Dashboard.Read", "Orders.Read", "Orders.Write", "Payments.Read", "Shipments.Manage", "Refunds.Manage", "Messages.Read"
        ]),
        new("WAREHOUSE_STAFF", "倉管人員", "負責庫存查詢、庫存調整、撿貨與出貨管理。",
        [
            "Dashboard.Read", "Inventory.Read", "Inventory.Adjust", "Orders.Read", "Shipments.Manage"
        ]),
        new("CUSTOMER_SERVICE", "客服人員", "負責查詢訂單、回覆訊息、協助付款與退款處理。",
        [
            "Dashboard.Read", "Orders.Read", "Payments.Read", "Refunds.Manage", "Messages.Read"
        ]),
        new("MARKETING_STAFF", "行銷人員", "負責優惠券、促銷活動與商品銷售觀察。",
        [
            "Dashboard.Read", "Analytics.Read", "Products.Read", "Coupons.Manage", "Promotions.Manage", "Messages.Read"
        ]),
        new("AUDIT_MANAGER", "稽核管理員", "負責查閱營運稽核紀錄、後台操作軌跡與分析讀取。",
        [
            "Dashboard.Read", "Analytics.Read", "AuditLogs.Read"
        ])
    ];

    private static readonly AdminUserSeed[] AdminUserSeeds =
    [
        new("ADM202606240001", "admin.super", "admin.super@freshmart.local", "系統管理員", "Admin", "SUPER_ADMIN"),
        new("ADM202606240002", "admin.operations", "admin.operations@freshmart.local", "營運主管", "Admin", "OPERATIONS_MANAGER"),
        new("ADM202606240003", "admin.product", "admin.product@freshmart.local", "商品管理員", "Staff", "PRODUCT_MANAGER"),
        new("ADM202606240004", "admin.order", "admin.order@freshmart.local", "訂單管理員", "Staff", "ORDER_MANAGER"),
        new("ADM202606240005", "admin.warehouse", "admin.warehouse@freshmart.local", "倉管人員", "Staff", "WAREHOUSE_STAFF"),
        new("ADM202606240006", "admin.service", "admin.service@freshmart.local", "客服人員", "Staff", "CUSTOMER_SERVICE"),
        new("ADM202606240007", "admin.marketing", "admin.marketing@freshmart.local", "行銷人員", "Staff", "MARKETING_STAFF"),
        new("ADM202606240008", "admin.audit", "admin.audit@freshmart.local", "稽核管理員", "Staff", "AUDIT_MANAGER")
    ];

    private sealed record PermissionSeed(string Code, string Name, string ModuleName, string Description);

    private sealed record RoleSeed(
        string Code,
        string Name,
        string Description,
        IReadOnlyCollection<string> PermissionCodes);

    private sealed record AdminUserSeed(
        string UserNo,
        string Account,
        string Email,
        string DisplayName,
        string UserType,
        string RoleCode);

    private static class PermissionCodes
    {
        public static readonly string[] All = AdminPermissionSeeds.Select(seed => seed.Code).ToArray();
    }
}

public static class AdminAuthSeederExtensions
{
    public static async Task SeedAdminAuthAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminAuthSeeder>();
        await seeder.SeedAsync();
    }
}
