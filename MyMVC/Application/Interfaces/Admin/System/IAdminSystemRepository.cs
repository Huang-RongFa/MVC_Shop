using MyWeb.Application.DTOs.Admin.System;

namespace MyWeb.Application.Interfaces.Admin.System;

public interface IAdminSystemRepository
{
    Task<AdminUserListResponse> GetUsersAsync(int page, int pageSize, CancellationToken cancellationToken);

    Task<AdminRolePermissionMatrixResponse> GetRolesAsync(CancellationToken cancellationToken);

    Task<AdminAuditTrailResponse> GetAuditTrailAsync(CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateUserStatusAsync(
        long actorUserId,
        long targetUserId,
        string status,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateUserRolesAsync(
        long actorUserId,
        long targetUserId,
        IReadOnlyCollection<string> roleCodes,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateRolePermissionsAsync(
        long actorUserId,
        int roleId,
        IReadOnlyCollection<string> permissionCodes,
        string? reason,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);
}
