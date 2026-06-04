using MyWeb.Data;
using MyWeb.Domain.Entities.Orders;
using MyWeb.Repositories.Orders;

namespace MyWeb.Infrastructure.Repositories.Orders;

public class CartRepository : EfRepositoryBase<ShoppingCart>, ICartRepository
{
    public CartRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class OrderRepository : EfRepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
