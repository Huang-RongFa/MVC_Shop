using Microsoft.EntityFrameworkCore;
using MyWeb.Application.Interfaces.Cart;
using MyWeb.Data;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Domain.Entities.Orders;

namespace MyWeb.Infrastructure.Repositories.Cart;

/// <summary>
/// 購物車資料存取。
/// 方法名稱刻意區分 ForRead / ForUpdate，提醒呼叫端哪些查詢會追蹤 Entity 並準備寫入。
/// </summary>
public sealed class CartRepository : ICartRepository
{
    private readonly AppDbContext _dbContext;

    public CartRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ProductSku?> GetDefaultPurchasableSkuAsync(
        long productId,
        CancellationToken cancellationToken)
    {
        // 加入購物車時只挑選啟用且未刪除的預設 SKU；正式多 SKU 選擇可擴充為由 request 指定 SkuId。
        return _dbContext.ProductSkus
            .AsNoTracking()
            .Include(sku => sku.Product)
            .Where(sku =>
                sku.ProductId == productId
                && sku.Status == "Active"
                && !sku.IsDeleted
                && sku.Product.Status == "Active"
                && !sku.Product.IsDeleted)
            .OrderBy(sku => sku.SkuId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<ShoppingCart?> GetActiveCartForUpdateAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        // 更新流程需要追蹤 Cart 與 Items，因此此查詢不使用 AsNoTracking。
        return _dbContext.ShoppingCarts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(
                cart => cart.UserId == userId && cart.CartStatus == "Active",
                cancellationToken);
    }

    public Task<ShoppingCart?> GetActiveCartForReadAsync(
        long userId,
        CancellationToken cancellationToken)
    {
        // 畫面讀取使用 AsNoTracking，並一次載入 SKU / Product，避免 ViewModel 組裝時產生 N+1 Query。
        return _dbContext.ShoppingCarts
            .AsNoTracking()
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Sku)
                    .ThenInclude(sku => sku.Product)
            .FirstOrDefaultAsync(
                cart => cart.UserId == userId && cart.CartStatus == "Active",
                cancellationToken);
    }

    public Task<ShoppingCartItem?> GetCartItemForUpdateAsync(
        long userId,
        long cartItemId,
        CancellationToken cancellationToken)
    {
        // 以 Cart.UserId 限制資料擁有者，Service 不需要另外信任前端傳入的 UserId。
        return _dbContext.ShoppingCartItems
            .Include(item => item.Cart)
            .FirstOrDefaultAsync(
                item =>
                    item.CartItemId == cartItemId
                    && item.Cart.UserId == userId
                    && item.Cart.CartStatus == "Active",
                cancellationToken);
    }

    public async Task<int> GetAvailableQuantityAsync(
        long skuId,
        CancellationToken cancellationToken)
    {
        // 可售庫存使用 InventoryStocks.AvailableQty 加總；建立訂單時仍需在 OrderService 交易中重新確認。
        return await _dbContext.InventoryStocks
            .AsNoTracking()
            .Where(stock => stock.SkuId == skuId)
            .SumAsync(stock => (int?)stock.AvailableQty, cancellationToken) ?? 0;
    }

    public void AddCart(ShoppingCart cart)
    {
        _dbContext.ShoppingCarts.Add(cart);
    }

    public void RemoveCartItem(ShoppingCartItem item)
    {
        _dbContext.ShoppingCartItems.Remove(item);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
