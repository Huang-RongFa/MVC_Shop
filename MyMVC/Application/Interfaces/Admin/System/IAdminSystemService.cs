using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.System;

namespace MyWeb.Application.Interfaces.Admin.System;

public interface IAdminSystemService
{
    Task<AdminUserListResponse> GetUsersAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminRolePermissionMatrixResponse> GetRolesAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken);

    Task<AdminAuditTrailResponse> GetAuditTrailAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken);

    Task<AdminSystemSettingsResponse> GetSettingsAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateUserStatusAsync(
        ClaimsPrincipal adminUser,
        long targetUserId,
        AdminUpdateUserStatusRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateUserRolesAsync(
        ClaimsPrincipal adminUser,
        long targetUserId,
        AdminUpdateUserRolesRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminMutationResponse> UpdateRolePermissionsAsync(
        ClaimsPrincipal adminUser,
        int roleId,
        AdminUpdateRolePermissionsRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);
}
