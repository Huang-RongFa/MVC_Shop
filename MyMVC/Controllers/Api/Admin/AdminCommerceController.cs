using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;
using MyWeb.Application.DTOs.Common;
using MyWeb.Application.Exceptions;
using MyWeb.Application.Interfaces.Admin.Commerce;
using MyWeb.Application.Security;

namespace MyWeb.Controllers.Api.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public sealed class AdminCommerceController : ControllerBase
{
    private readonly IAdminCommerceService _adminCommerceService;

    public AdminCommerceController(IAdminCommerceService adminCommerceService)
    {
        _adminCommerceService = adminCommerceService;
    }

    [HttpGet("products")]
    [Authorize(Policy = AdminPermissionCodes.ProductsRead)]
    public async Task<ActionResult<ApiResponse<AdminProductListResponse>>> Products(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetProductsAsync(
                User,
                page,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<AdminProductListResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("products/edit-options")]
    [Authorize(Policy = AdminPermissionCodes.ProductsWrite)]
    public async Task<ActionResult<ApiResponse<AdminProductEditResponse>>> ProductCreateOptions(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetProductCreateOptionsAsync(User, cancellationToken);
            return Ok(ApiResponse<AdminProductEditResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("products/{productId:long}/edit")]
    [Authorize(Policy = AdminPermissionCodes.ProductsWrite)]
    public async Task<ActionResult<ApiResponse<AdminProductEditResponse>>> ProductForEdit(
        long productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetProductForEditAsync(
                User,
                productId,
                cancellationToken);

            return Ok(ApiResponse<AdminProductEditResponse>.Success(response));
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
            return NotFound(ApiResponse.Failure("ADMIN_PRODUCT_NOT_FOUND", "找不到指定的商品。"));
        }
    }

    [HttpPost("products")]
    [Authorize(Policy = AdminPermissionCodes.ProductsWrite)]
    public async Task<ActionResult<ApiResponse<AdminProductMutationResponse>>> CreateProduct(
        [FromBody] AdminProductUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.CreateProductAsync(
                User,
                request,
                BuildRequestContext(),
                cancellationToken);

            return CreatedAtAction(
                nameof(ProductForEdit),
                new { productId = response.ProductId },
                ApiResponse<AdminProductMutationResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (AdminSystemValidationException ex)
        {
            return BadRequest(ApiResponse.Failure(ex.Code, ex.Message));
        }
    }

    [HttpPut("products/{productId:long}")]
    [Authorize(Policy = AdminPermissionCodes.ProductsWrite)]
    public async Task<ActionResult<ApiResponse<AdminProductMutationResponse>>> UpdateProduct(
        long productId,
        [FromBody] AdminProductUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.UpdateProductAsync(
                User,
                productId,
                request,
                BuildRequestContext(),
                cancellationToken);

            return Ok(ApiResponse<AdminProductMutationResponse>.Success(response));
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
            return NotFound(ApiResponse.Failure("ADMIN_PRODUCT_NOT_FOUND", "找不到指定的商品。"));
        }
    }

    [HttpGet("products/skus")]
    [Authorize(Policy = AdminPermissionCodes.ProductsWrite)]
    public async Task<ActionResult<ApiResponse<AdminSkuListResponse>>> ProductSkus(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetSkusAsync(
                User,
                page,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<AdminSkuListResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("inventory")]
    [Authorize(Policy = AdminPermissionCodes.InventoryRead)]
    public async Task<ActionResult<ApiResponse<AdminInventoryListResponse>>> Inventory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetInventoryAsync(
                User,
                page,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<AdminInventoryListResponse>.Success(response));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("orders")]
    [Authorize(Policy = AdminPermissionCodes.OrdersRead)]
    public async Task<ActionResult<ApiResponse<AdminOrderListResponse>>> Orders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _adminCommerceService.GetOrdersAsync(
                User,
                page,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<AdminOrderListResponse>.Success(response));
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
