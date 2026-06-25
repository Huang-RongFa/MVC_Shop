using Microsoft.EntityFrameworkCore;
using MyWeb.Application.Interfaces.Catalog;
using MyWeb.Data;

namespace MyWeb.Infrastructure.Repositories.Catalog;

public sealed class ProductStockRepository : IProductStockRepository
{
    private readonly AppDbContext _dbContext;

    public ProductStockRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<long, int>> GetAvailableStockByProductIdsAsync(
        IReadOnlyCollection<long> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return new Dictionary<long, int>();
        }

        var rows = await _dbContext.ProductSkus
            .AsNoTracking()
            .Where(sku =>
                productIds.Contains(sku.ProductId)
                && sku.Status == "Active"
                && !sku.IsDeleted
                && sku.Product.Status == "Active"
                && !sku.Product.IsDeleted)
            .Select(sku => new
            {
                sku.ProductId,
                AvailableQuantity = _dbContext.InventoryStocks
                    .Where(stock => stock.SkuId == sku.SkuId)
                    .Sum(stock => (int?)stock.AvailableQty) ?? 0
            })
            .ToArrayAsync(cancellationToken);

        return rows
            .GroupBy(row => row.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group.Sum(row => row.AvailableQuantity));
    }
}
