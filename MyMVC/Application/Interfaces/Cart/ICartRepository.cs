using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Orders;

namespace MyWeb.Application.Interfaces.Cart;

public interface ICartRepository
{
    Task<ProductSku?> GetDefaultPurchasableSkuAsync(
        long productId,
        CancellationToken cancellationToken);

    Task<ShoppingCart?> GetActiveCartForUpdateAsync(
        long userId,
        CancellationToken cancellationToken);

    Task<ShoppingCart?> GetActiveCartForReadAsync(
        long userId,
        CancellationToken cancellationToken);

    Task<ShoppingCartItem?> GetCartItemForUpdateAsync(
        long userId,
        long cartItemId,
        CancellationToken cancellationToken);

    Task<int> GetAvailableQuantityAsync(
        long skuId,
        CancellationToken cancellationToken);

    void AddCart(ShoppingCart cart);

    void RemoveCartItem(ShoppingCartItem item);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
