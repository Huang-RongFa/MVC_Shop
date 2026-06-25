using MyWeb.Application.DTOs.Admin.Commerce;
using MyWeb.Application.DTOs.Admin.System;

namespace MyWeb.Application.Interfaces.Admin.Commerce;

public interface IAdminCommerceRepository
{
    Task<AdminProductListResponse> GetProductsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminProductEditResponse> GetProductCreateOptionsAsync(CancellationToken cancellationToken);

    Task<AdminProductEditResponse> GetProductForEditAsync(
        long productId,
        CancellationToken cancellationToken);

    Task<AdminProductMutationResponse> CreateProductAsync(
        long actorUserId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminProductMutationResponse> UpdateProductAsync(
        long actorUserId,
        long productId,
        AdminProductUpsertRequest request,
        AdminSystemRequestContext requestContext,
        CancellationToken cancellationToken);

    Task<AdminSkuListResponse> GetSkusAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminInventoryListResponse> GetInventoryAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<AdminOrderListResponse> GetOrdersAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
