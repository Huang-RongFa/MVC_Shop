namespace MyWeb.Application.Interfaces.Catalog;

public interface IProductStockRepository
{
    Task<IReadOnlyDictionary<long, int>> GetAvailableStockByProductIdsAsync(
        IReadOnlyCollection<long> productIds,
        CancellationToken cancellationToken);
}
