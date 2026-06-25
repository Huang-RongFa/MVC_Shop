using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.DTOs.Common;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.System;
using MyWeb.Application.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/system")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminSystemController : ControllerBase
{
    private readonly IAdminSystemService _adminSystemService;

    public AdminSystemController(IAdminSystemService adminSystemService)
    {
        _adminSystemService = adminSystemService;
    }

    [HttpGet("users")]
    [Authorize(Policy = AdminPermissionCodes.UsersManage)]
    public async Task<ActionResult<ApiResponse<AdminUserListResponse>>> Users(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminSystemService.GetUsersAsync(User, page, pageSize, cancellationToken);
            return Ok(ApiResponse<AdminUserListResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPatch("users/{userId:long}/status")]
    [Authorize(Policy = AdminPermissionCodes.UsersManage)]
    public async Task<ActionResult<ApiResponse<AdminMutationResponse>>> UpdateUserStatus(
        long userId,
        [FromBody] AdminUpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.UpdateUserStatusAsync(
                User,
                userId,
                request,
                BuildRequestContext(),
                cancellationToken);

            return Ok(ApiResponse<AdminMutationResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (AdminSystemValidationException ex)
        {
            return BadRequest(ApiResponse.Failure(ex.Code, ex.Message));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Failure("ADMIN_USER_NOT_FOUND", "找不到指定的使用者。"));
        }
    }

    [HttpPut("users/{userId:long}/roles")]
    [Authorize(Policy = AdminPermissionCodes.UsersManage)]
    public async Task<ActionResult<ApiResponse<AdminMutationResponse>>> UpdateUserRoles(
        long userId,
        [FromBody] AdminUpdateUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.UpdateUserRolesAsync(
                User,
                userId,
                request,
                BuildRequestContext(),
                cancellationToken);

            return Ok(ApiResponse<AdminMutationResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (AdminSystemValidationException ex)
        {
            return BadRequest(ApiResponse.Failure(ex.Code, ex.Message));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Failure("ADMIN_USER_NOT_FOUND", "找不到指定的使用者。"));
        }
    }

    [HttpGet("roles")]
    [Authorize(Policy = AdminPermissionCodes.RolesManage)]
    public async Task<ActionResult<ApiResponse<AdminRolePermissionMatrixResponse>>> Roles(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.GetRolesAsync(User, cancellationToken);
            return Ok(ApiResponse<AdminRolePermissionMatrixResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("roles/{roleId:int}/permissions")]
    [Authorize(Policy = AdminPermissionCodes.RolesManage)]
    public async Task<ActionResult<ApiResponse<AdminMutationResponse>>> UpdateRolePermissions(
        int roleId,
        [FromBody] AdminUpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.UpdateRolePermissionsAsync(
                User,
                roleId,
                request,
                BuildRequestContext(),
                cancellationToken);

            return Ok(ApiResponse<AdminMutationResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (AdminSystemValidationException ex)
        {
            return BadRequest(ApiResponse.Failure(ex.Code, ex.Message));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(ApiResponse.Failure("ADMIN_ROLE_NOT_FOUND", "找不到指定的角色。"));
        }
    }

    [HttpGet("audit-trail")]
    [HttpGet("/api/admin/audit-logs")]
    [Authorize(Policy = AdminPermissionCodes.AuditLogsRead)]
    public async Task<ActionResult<ApiResponse<AdminAuditTrailResponse>>> AuditTrail(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.GetAuditTrailAsync(User, cancellationToken);
            return Ok(ApiResponse<AdminAuditTrailResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("settings")]
    [HttpGet("/api/admin/settings")]
    [Authorize(Policy = AdminPermissionCodes.SettingsManage)]
    public async Task<ActionResult<ApiResponse<AdminSystemSettingsResponse>>> Settings(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _adminSystemService.GetSettingsAsync(User, cancellationToken);
            return Ok(ApiResponse<AdminSystemSettingsResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private AdminSystemRequestContext BuildRequestContext()
    {
        return new AdminSystemRequestContext(
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString());
    }
}
