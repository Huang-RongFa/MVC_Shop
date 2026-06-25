namespace MyWeb.Application.DTOs.Admin.System;

public sealed record AdminUserListResponse(
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    IReadOnlyCollection<AdminUserListItemDto> Users,
    IReadOnlyCollection<AdminRoleOptionDto>? RoleOptions = null,
    IReadOnlyCollection<string>? AvailableStatuses = null);

public sealed record AdminUserListItemDto(
    long UserId,
    string UserNo,
    string Account,
    string? Email,
    string DisplayName,
    string UserType,
    string Status,
    IReadOnlyCollection<string> Roles,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    IReadOnlyCollection<string>? RoleCodes = null);

public sealed record AdminRoleOptionDto(
    int RoleId,
    string RoleCode,
    string RoleName,
    bool IsSystemRole);

public sealed record AdminRolePermissionMatrixResponse(
    int RoleCount,
    int PermissionCount,
    IReadOnlyCollection<AdminRoleListItemDto> Roles,
    IReadOnlyCollection<AdminPermissionGroupDto> PermissionGroups);

public sealed record AdminRoleListItemDto(
    int RoleId,
    string RoleCode,
    string RoleName,
    string? Description,
    bool IsSystemRole,
    int UserCount,
    IReadOnlyCollection<string> Permissions);

public sealed record AdminPermissionGroupDto(
    string ModuleName,
    IReadOnlyCollection<AdminPermissionDto> Permissions);

public sealed record AdminPermissionDto(
    int PermissionId,
    string PermissionCode,
    string PermissionName,
    string ModuleName,
    string? Description);

public sealed record AdminAuditTrailResponse(
    IReadOnlyCollection<AdminActionLogItemDto> AdminActions,
    IReadOnlyCollection<AdminDataChangeLogItemDto> DataChanges,
    IReadOnlyCollection<AdminSystemErrorLogItemDto> SystemErrors);

public sealed record AdminActionLogItemDto(
    long AdminActionLogId,
    long UserId,
    string AdminDisplayName,
    string ModuleName,
    string ActionName,
    string? TargetType,
    string? TargetId,
    string? Description,
    string? IpAddress,
    DateTime CreatedAt);

public sealed record AdminDataChangeLogItemDto(
    long AuditLogId,
    string TableName,
    string RecordId,
    string ActionType,
    long? ChangedBy,
    string? ChangedByDisplayName,
    string? IpAddress,
    DateTime ChangedAt);

public sealed record AdminSystemErrorLogItemDto(
    long ErrorLogId,
    string ErrorLevel,
    string? Source,
    string Message,
    string? RequestPath,
    long? UserId,
    DateTime CreatedAt);

public sealed record AdminSystemSettingsResponse(
    IReadOnlyCollection<AdminSystemSettingSectionDto> Sections);

public sealed record AdminSystemSettingSectionDto(
    string SectionKey,
    string SectionName,
    IReadOnlyCollection<AdminSystemSettingItemDto> Items);

public sealed record AdminSystemSettingItemDto(
    string Key,
    string Name,
    string Value,
    bool IsImplemented,
    string RiskLevel,
    string Description);

public sealed record AdminSystemRequestContext(
    string? IpAddress,
    string? UserAgent);

public sealed record AdminUpdateUserStatusRequest(
    string Status,
    string? Reason);

public sealed record AdminUpdateUserRolesRequest(
    IReadOnlyCollection<string> RoleCodes,
    string? Reason);

public sealed record AdminUpdateRolePermissionsRequest(
    IReadOnlyCollection<string> PermissionCodes,
    string? Reason);

public sealed record AdminMutationResponse(
    string Message);
