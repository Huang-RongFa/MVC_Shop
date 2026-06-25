using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.System;
using MyWeb.Application.Security;

namespace MyWeb.Application.Services.Admin.System;

/// <summary>
/// 後台帳號、角色權限、稽核與系統設定的商業流程。
/// Controller 的 Policy 是第一層保護；此 Service 會再次檢查 Permission，
/// 並負責輸入正規化與避免高權限操作誤傷自己或系統角色。
/// </summary>
public sealed class AdminSystemService : IAdminSystemService
{
    // 後台列表固定允許的分頁大小，避免使用者以任意 pageSize 拖垮管理查詢。
    private static readonly HashSet<int> AllowedPageSizes = [10, 20, 50];

    private static readonly HashSet<string> ValidUserStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Active",
        "Inactive",
        "Locked"
    };

    private readonly IAdminSystemRepository _adminSystemRepository;

    public AdminSystemService(IAdminSystemRepository adminSystemRepository)
    {
        _adminSystemRepository = adminSystemRepository;
    }

    public Task<AdminUserListResponse> GetUsersAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.UsersManage);

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = AllowedPageSizes.Contains(pageSize) ? pageSize : 10;

        // Repository 只接收已正規化的分頁參數，不決定後台分頁策略。
        return _adminSystemRepository.GetUsersAsync(normalizedPage, normalizedPageSize, cancellationToken);
    }

    public Task<AdminRolePermissionMatrixResponse> GetRolesAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.RolesManage);
        return _adminSystemRepository.GetRolesAsync(cancellationToken);
    }

    public Task<AdminAuditTrailResponse> GetAuditTrailAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.AuditLogsRead);
        return _adminSystemRepository.GetAuditTrailAsync(cancellationToken);
    }

    public Task<AdminSystemSettingsResponse> GetSettingsAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.SettingsManage);

        // 目前尚未建立 SystemSettings 資料表，因此只回傳正式上線設定狀態與待辦提示。
        var response = new AdminSystemSettingsResponse(
        [
            new AdminSystemSettingSectionDto(
                "security",
                "安全設定",
                [
                    new AdminSystemSettingItemDto(
                        "cookie-auth",
                        "Cookie Authentication",
                        "HttpOnly / Secure in production / SameSite=Lax",
                        true,
                        "medium",
                        "正式環境以 Cookie Authentication 做登入狀態，避免高權限 token 長期放在瀏覽器儲存。"),
                    new AdminSystemSettingItemDto(
                        "csrf",
                        "CSRF Protection",
                        "X-CSRF-TOKEN",
                        true,
                        "medium",
                        "寫入型 API 需帶 antiforgery token，降低跨站請求偽造風險。")
                ]),
            new AdminSystemSettingSectionDto(
                "audit",
                "稽核設定",
                [
                    new AdminSystemSettingItemDto(
                        "login-logs",
                        "Login Logs",
                        "UserLoginLogs",
                        true,
                        "low",
                        "後台登入成功與失敗皆保留登入紀錄。"),
                    new AdminSystemSettingItemDto(
                        "admin-actions",
                        "Admin Action Logs",
                        "AdminActionLogs",
                        true,
                        "medium",
                        "高權限操作應寫入後台操作紀錄，目前資料表與查詢已建立，寫入流程需在各業務 Service 落實。")
                ]),
            new AdminSystemSettingSectionDto(
                "operations",
                "營運設定",
                [
                    new AdminSystemSettingItemDto(
                        "editable-system-settings",
                        "Editable System Settings",
                        "NotConfigured",
                        false,
                        "medium",
                        "目前尚未建立 SystemSettings 資料表，因此只提供唯讀設計狀態。")
                ])
        ]);

        return Task.FromResult(response);
    }

    public Task<AdminMutationResponse> UpdateUserStatusAsync(
        ClaimsPrincipal adminUser,
        long targetUserId,
        AdminUpdateUserStatusRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.UsersManage);
        var actorUserId = GetActorUserId(adminUser);
        var status = NormalizeRequired(request.Status, "ADMIN_USER_STATUS_REQUIRED", "請選擇使用者狀態。");

        if (!ValidUserStatuses.Contains(status))
        {
            throw new AdminSystemValidationException("ADMIN_USER_STATUS_INVALID", "使用者狀態不符合系統允許值。");
        }

        if (actorUserId == targetUserId && !string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            // 避免管理者停用或鎖定自己造成後台無法操作；更完整策略可再加入 Break-glass 管理員。
            throw new AdminSystemValidationException("ADMIN_CANNOT_DISABLE_SELF", "不可停用或鎖定自己的後台帳號。");
        }

        return _adminSystemRepository.UpdateUserStatusAsync(
            actorUserId,
            targetUserId,
            status,
            NormalizeOptional(request.Reason),
            requestContext,
            cancellationToken);
    }

    public Task<AdminMutationResponse> UpdateUserRolesAsync(
        ClaimsPrincipal adminUser,
        long targetUserId,
        AdminUpdateUserRolesRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.UsersManage);
        var actorUserId = GetActorUserId(adminUser);
        var roleCodes = NormalizeDistinctCodes(request.RoleCodes);

        if (roleCodes.Count == 0)
        {
            // 後台使用者至少保留一個角色，避免產生無法管理或權限狀態不明的帳號。
            throw new AdminSystemValidationException("ADMIN_USER_ROLE_REQUIRED", "至少需要指派一個角色。");
        }

        return _adminSystemRepository.UpdateUserRolesAsync(
            actorUserId,
            targetUserId,
            roleCodes,
            NormalizeOptional(request.Reason),
            requestContext,
            cancellationToken);
    }

    public Task<AdminMutationResponse> UpdateRolePermissionsAsync(
        ClaimsPrincipal adminUser,
        int roleId,
        AdminUpdateRolePermissionsRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken)
    {
        EnsurePermission(adminUser, AdminPermissionCodes.RolesManage);
        var actorUserId = GetActorUserId(adminUser);
        var permissionCodes = NormalizeDistinctCodes(request.PermissionCodes);

        if (permissionCodes.Count == 0)
        {
            // 角色不允許被清成完全無權限，降低誤操作導致管理流程中斷的風險。
            throw new AdminSystemValidationException("ADMIN_ROLE_PERMISSION_REQUIRED", "至少需要保留一個權限。");
        }

        return _adminSystemRepository.UpdateRolePermissionsAsync(
            actorUserId,
            roleId,
            permissionCodes,
            NormalizeOptional(request.Reason),
            requestContext,
            cancellationToken);
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

    private static long GetActorUserId(ClaimsPrincipal user)
    {
        var userIdText = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(userIdText, out var userId) || userId <= 0)
        {
            throw new AdminSystemValidationException("ADMIN_ACTOR_INVALID", "無法辨識目前後台操作者。");
        }

        return userId;
    }

    private static string NormalizeRequired(string value, string code, string message)
    {
        var normalized = value.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new AdminSystemValidationException(code, message);
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static IReadOnlyCollection<string> NormalizeDistinctCodes(IReadOnlyCollection<string>? codes)
    {
        // 權限與角色代碼先去空白、去重與排序，讓稽核紀錄穩定且易於比對。
        return (codes ?? [])
            .Select(code => code.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(code => code)
            .ToArray();
    }
}
