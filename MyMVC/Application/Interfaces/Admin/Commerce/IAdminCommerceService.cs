using System.Security.Claims;
using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;

namespace MyWeb.Application.Interfaces.Admin.Commerce;

public interface IAdminCommerceService
{
    Task<AdminProductListResponse> GetProductsAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminProductEditResponse> GetProductCreateOptionsAsync(
        ClaimsPrincipal adminUser,
        CancellationToken cancellationToken);

    Task<AdminProductEditResponse> GetProductForEditAsync(
        ClaimsPrincipal adminUser,
        long productId,
        CancellationToken cancellationToken);

    Task<AdminProductMutationResponse> CreateProductAsync(
        ClaimsPrincipal adminUser,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminProductMutationResponse> UpdateProductAsync(
        ClaimsPrincipal adminUser,
        long productId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminSkuListResponse> GetSkusAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminInventoryListResponse> GetInventoryAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminOrderListResponse> GetOrdersAsync(
        ClaimsPrincipal adminUser,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
