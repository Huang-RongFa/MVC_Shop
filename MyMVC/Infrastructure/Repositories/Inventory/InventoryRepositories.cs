using MyWeb.Data;
using MyWeb.Domain.Entities.Inventory;
using MyWeb.Repositories.Inventory;

namespace MyWeb.Infrastructure.Repositories.Inventory;

public class InventoryRepository : EfRepositoryBase<InventoryStock>, IInventoryRepository
{
    public InventoryRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class WarehouseRepository : EfRepositoryBase<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
