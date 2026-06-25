using Microsoft.EntityFrameworkCore;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.System;
using MyWeb.Data;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.Domain.Entities.Logs;

namespace MyWeb.Infrastructure.Repositories.Admin.System;

/// <summary>
/// 後台系統管理資料存取。
/// 查詢方法使用投影 DTO 與 AsNoTracking；寫入方法負責同一交易內更新資料並寫入 AdminActionLogs。
/// </summary>
public sealed class AdminSystemRepository : IAdminSystemRepository
{
    private const int DefaultAuditLimit = 50;
    private readonly AppDbContext _dbContext;

    public AdminSystemRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminUserListResponse> GetUsersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var totalCount = await _dbContext.Users
            .AsNoTracking()
            .Where(user => !user.IsDeleted)
            .CountAsync(cancellationToken);

        var totalPages = totalCount == 0
            ? 1
            : (int)Math.Ceiling(totalCount / (double)pageSize);
        var normalizedPage = Math.Min(Math.Max(page, 1), totalPages);
        var skip = (normalizedPage - 1) * pageSize;

        // 使用 Select 直接投影 DTO，避免將 PasswordHash、SecurityStamp 等敏感欄位載入回應流程。
        var users = await _dbContext.Users
            .AsNoTracking()
            .Where(user => !user.IsDeleted)
            .OrderByDescending(user => user.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(user => new AdminUserListItemDto(
                user.UserId,
                user.UserNo,
                user.Account,
                user.Email,
                user.DisplayName,
                user.UserType,
                user.Status,
                user.UserRoles
                    .Where(userRole => !userRole.Role.IsDeleted)
                    .Select(userRole => userRole.Role.RoleName)
                    .OrderBy(roleName => roleName)
                    .ToArray(),
                user.LastLoginAt,
                user.CreatedAt,
                user.UserRoles
                    .Where(userRole => !userRole.Role.IsDeleted)
                    .Select(userRole => userRole.Role.RoleCode)
                    .OrderBy(roleCode => roleCode)
                    .ToArray()))
            .ToListAsync(cancellationToken);

        var roleOptions = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => !role.IsDeleted)
            .OrderBy(role => role.RoleName)
            .Select(role => new AdminRoleOptionDto(
                role.RoleId,
                role.RoleCode,
                role.RoleName,
                role.IsSystemRole))
            .ToListAsync(cancellationToken);

        return new AdminUserListResponse(
            totalCount,
            normalizedPage,
            pageSize,
            totalPages,
            users,
            roleOptions,
            ["Active", "Inactive", "Locked"]);
    }

    public async Task<AdminRolePermissionMatrixResponse> GetRolesAsync(CancellationToken cancellationToken)
    {
        var roles = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => !role.IsDeleted)
            .OrderBy(role => role.RoleCode)
            .Select(role => new AdminRoleListItemDto(
                role.RoleId,
                role.RoleCode,
                role.RoleName,
                role.Description,
                role.IsSystemRole,
                role.UserRoles.Count(userRole => !userRole.User.IsDeleted),
                role.RolePermissions
                    .Select(rolePermission => rolePermission.Permission.PermissionCode)
                    .OrderBy(permissionCode => permissionCode)
                    .ToArray()))
            .ToListAsync(cancellationToken);

        var permissions = await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(permission => permission.ModuleName)
            .ThenBy(permission => permission.PermissionCode)
            .Select(permission => new AdminPermissionDto(
                permission.PermissionId,
                permission.PermissionCode,
                permission.PermissionName,
                permission.ModuleName,
                permission.Description))
            .ToListAsync(cancellationToken);

        var permissionGroups = permissions
            .GroupBy(permission => permission.ModuleName)
            .Select(group => new AdminPermissionGroupDto(group.Key, group.ToArray()))
            .ToArray();

        return new AdminRolePermissionMatrixResponse(
            roles.Count,
            permissions.Count,
            roles,
            permissionGroups);
    }

    public async Task<AdminAuditTrailResponse> GetAuditTrailAsync(CancellationToken cancellationToken)
    {
        // 稽核查詢只取最近固定筆數，避免後台頁面初載入直接掃描完整 Log 表。
        var adminActions = await _dbContext.AdminActionLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(DefaultAuditLimit)
            .Select(log => new AdminActionLogItemDto(
                log.AdminActionLogId,
                log.UserId,
                log.User.DisplayName,
                log.ModuleName,
                log.ActionName,
                log.TargetType,
                log.TargetId,
                log.Description,
                log.IpAddress,
                log.CreatedAt))
            .ToListAsync(cancellationToken);

        var dataChanges = await _dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.ChangedAt)
            .Take(DefaultAuditLimit)
            .Select(log => new AdminDataChangeLogItemDto(
                log.AuditLogId,
                log.TableName,
                log.RecordId,
                log.ActionType,
                log.ChangedBy,
                log.ChangedByUser == null ? null : log.ChangedByUser.DisplayName,
                log.IpAddress,
                log.ChangedAt))
            .ToListAsync(cancellationToken);

        var systemErrorRows = await _dbContext.SystemErrorLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Take(DefaultAuditLimit)
            .Select(log => new
            {
                log.ErrorLogId,
                log.ErrorLevel,
                log.Source,
                log.Message,
                log.RequestPath,
                log.UserId,
                log.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var systemErrors = systemErrorRows
            .Select(log => new AdminSystemErrorLogItemDto(
                log.ErrorLogId,
                log.ErrorLevel,
                log.Source,
                // 系統錯誤顯示前先淨化摘要，避免 SQL、路徑或內部資訊出現在後台頁面。
                BuildSafeSystemErrorMessage(log.Message),
                log.RequestPath,
                log.UserId,
                log.CreatedAt))
            .ToList();

        return new AdminAuditTrailResponse(adminActions, dataChanges, systemErrors);
    }

    public async Task<AdminMutationResponse> UpdateUserStatusAsync(
        long actorUserId,
        long targetUserId,
        string status,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        // EF SQL Server retry strategy 需要把交易包進 ExecuteAsync，避免暫時性錯誤重試時交易狀態不一致。
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;

            var user = await _dbContext.Users
                .FirstOrDefaultAsync(item => item.UserId == targetUserId && !item.IsDeleted, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("ADMIN_USER_NOT_FOUND");
            }

            var oldStatus = user.Status;
            user.Status = status;
            user.UpdatedAt = now;

            // 狀態異動與稽核紀錄在同一交易提交，確保操作可追蹤。
            _dbContext.AdminActionLogs.Add(BuildActionLog(
                actorUserId,
                "Accounts",
                "UpdateUserStatus",
                "User",
                targetUserId.ToString(),
                $"Status: {oldStatus} -> {status}. Reason: {reason ?? "N/A"}",
                requestContext,
                now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AdminMutationResponse("使用者狀態已更新。");
        });
    }

    public async Task<AdminMutationResponse> UpdateUserRolesAsync(
        long actorUserId,
        long targetUserId,
        IReadOnlyCollection<string> roleCodes,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var normalizedRoleCodes = roleCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 角色異動屬高權限操作，資料更新與 AdminActionLogs 必須一起成功或一起回滾。
            var user = await _dbContext.Users
                .Include(item => item.UserRoles)
                    .ThenInclude(userRole => userRole.Role)
                .FirstOrDefaultAsync(item => item.UserId == targetUserId && !item.IsDeleted, cancellationToken);

            if (user is null)
            {
                throw new KeyNotFoundException("ADMIN_USER_NOT_FOUND");
            }

            if (string.Equals(user.UserType, "Customer", StringComparison.OrdinalIgnoreCase))
            {
                throw new AdminSystemValidationException(
                    "ADMIN_USER_ROLE_TARGET_INVALID",
                    "前台會員不可直接指派後台角色。");
            }

            var roles = await _dbContext.Roles
                .Where(role => normalizedRoleCodes.Contains(role.RoleCode) && !role.IsDeleted)
                .ToListAsync(cancellationToken);

            if (roles.Count != normalizedRoleCodes.Count)
            {
                throw new AdminSystemValidationException("ADMIN_ROLE_NOT_FOUND", "角色清單包含不存在或已停用的角色。");
            }

            var targetRoleIds = roles.Select(role => role.RoleId).ToHashSet();
            var removeItems = user.UserRoles
                .Where(userRole => !targetRoleIds.Contains(userRole.RoleId))
                .ToArray();

            _dbContext.UserRoles.RemoveRange(removeItems);

            var currentRoleIds = user.UserRoles
                .Where(userRole => !removeItems.Contains(userRole))
                .Select(userRole => userRole.RoleId)
                .ToHashSet();

            foreach (var role in roles.Where(role => !currentRoleIds.Contains(role.RoleId)))
            {
                _dbContext.UserRoles.Add(new UserRole
                {
                    UserId = targetUserId,
                    RoleId = role.RoleId,
                    AssignedAt = now,
                    AssignedBy = actorUserId
                });
            }

            user.UpdatedAt = now;

            _dbContext.AdminActionLogs.Add(BuildActionLog(
                actorUserId,
                "Accounts",
                "UpdateUserRoles",
                "User",
                targetUserId.ToString(),
                $"Roles: {string.Join(", ", normalizedRoleCodes.OrderBy(code => code))}. Reason: {reason ?? "N/A"}",
                requestContext,
                now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AdminMutationResponse("使用者角色已更新。");
        });
    }

    public async Task<AdminMutationResponse> UpdateRolePermissionsAsync(
        long actorUserId,
        int roleId,
        IReadOnlyCollection<string> permissionCodes,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
        return await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var normalizedPermissionCodes = permissionCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 權限矩陣異動不允許部分成功；若任何 Permission 不存在，整筆交易回滾。
            var role = await _dbContext.Roles
                .Include(item => item.RolePermissions)
                    .ThenInclude(rolePermission => rolePermission.Permission)
                .FirstOrDefaultAsync(item => item.RoleId == roleId && !item.IsDeleted, cancellationToken);

            if (role is null)
            {
                throw new KeyNotFoundException("ADMIN_ROLE_NOT_FOUND");
            }

            if (string.Equals(role.RoleCode, "SUPER_ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                throw new AdminSystemValidationException(
                    "ADMIN_SUPER_ADMIN_ROLE_IMMUTABLE",
                    "不可修改系統最高管理員角色的權限。");
            }

            var permissions = await _dbContext.Permissions
                .Where(permission => normalizedPermissionCodes.Contains(permission.PermissionCode))
                .ToListAsync(cancellationToken);

            if (permissions.Count != normalizedPermissionCodes.Count)
            {
                throw new AdminSystemValidationException("ADMIN_PERMISSION_NOT_FOUND", "權限清單包含不存在的權限。");
            }

            var targetPermissionIds = permissions.Select(permission => permission.PermissionId).ToHashSet();
            var removeItems = role.RolePermissions
                .Where(rolePermission => !targetPermissionIds.Contains(rolePermission.PermissionId))
                .ToArray();

            _dbContext.RolePermissions.RemoveRange(removeItems);

            var currentPermissionIds = role.RolePermissions
                .Where(rolePermission => !removeItems.Contains(rolePermission))
                .Select(rolePermission => rolePermission.PermissionId)
                .ToHashSet();

            foreach (var permission in permissions.Where(permission => !currentPermissionIds.Contains(permission.PermissionId)))
            {
                _dbContext.RolePermissions.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permission.PermissionId,
                    CreatedAt = now
                });
            }

            _dbContext.AdminActionLogs.Add(BuildActionLog(
                actorUserId,
                "Accounts",
                "UpdateRolePermissions",
                "Role",
                roleId.ToString(),
                $"RoleCode: {role.RoleCode}. Permissions: {string.Join(", ", normalizedPermissionCodes.OrderBy(code => code))}. Reason: {reason ?? "N/A"}",
                requestContext,
                now));

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new AdminMutationResponse("角色權限已更新。");
        });
    }

    private static AdminActionLog BuildActionLog(
        long actorUserId,
        string moduleName,
        string actionName,
        string targetType,
        string targetId,
        string description,
        AdminSystemRequestContext requestContext,
        DateTime now)
    {
        return new AdminActionLog
        {
            UserId = actorUserId,
            ModuleName = moduleName,
            ActionName = actionName,
            TargetType = targetType,
            TargetId = targetId,
            Description = description,
            IpAddress = requestContext.IpAddress,
            UserAgent = requestContext.UserAgent,
            CreatedAt = now
        };
    }

    private static string BuildSafeSystemErrorMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "未記錄錯誤摘要。";
        }

        var normalized = message.Trim();
        var sensitiveTokens = new[]
        {
            "SELECT ",
            "UPDATE ",
            "DELETE ",
            "INSERT ",
            "CONNECTION STRING",
            "MICROSOFT.DATA.SQLCLIENT",
            "SYSTEM.DATA.SQLCLIENT",
            ":\\",
            "\\",
            "/HOME/",
            "/APP/"
        };

        if (sensitiveTokens.Any(token => normalized.Contains(token, StringComparison.OrdinalIgnoreCase)))
        {
            return "系統錯誤摘要已記錄，詳細內容保留於受控伺服器紀錄。";
        }

        const int maxLength = 180;
        return normalized.Length <= maxLength
            ? normalized
            : string.Concat(normalized.AsSpan(0, maxLength - 3), "...");
    }
}
