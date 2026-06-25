namespace MyWeb.Application.DTOs.Storefront.Cart;

public sealed class AddCartItemRequest
{
    public long ProductId { get; init; }

    public int Quantity { get; init; } = 1;

    public string? ReturnUrl { get; init; }
}
