using System.Security.Claims;
using MyWeb.Application.DTOs.Storefront.Cart;
using MyWeb.Models.ViewModels.Storefront;

namespace MyWeb.Application.Interfaces.Cart;

public interface ICartService
{
    Task<CartOperationResult> AddItemAsync(
        ClaimsPrincipal user,
        AddCartItemRequest request,
        CancellationToken cancellationToken);

    Task<CartOperationResult> UpdateQuantityAsync(
        ClaimsPrincipal user,
        long cartItemId,
        int quantity,
        CancellationToken cancellationToken);

    Task<CartOperationResult> RemoveItemAsync(
        ClaimsPrincipal user,
        long cartItemId,
        CancellationToken cancellationToken);

    Task<CartViewModel> GetCartAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken);

    Task<CartCouponPreviewResult> PreviewCouponAsync(
        ClaimsPrincipal user,
        string? couponCode,
        CancellationToken cancellationToken);
}

public sealed class CartOperationResult
{
    private CartOperationResult(bool succeeded, string message)
    {
        Succeeded = succeeded;
        Message = message;
    }

    public bool Succeeded { get; }

    public string Message { get; }

    public static CartOperationResult Success(string message)
    {
        return new CartOperationResult(true, message);
    }

    public static CartOperationResult Failure(string message)
    {
        return new CartOperationResult(false, message);
    }
}
