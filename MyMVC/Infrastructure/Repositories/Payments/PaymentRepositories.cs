using MyWeb.Data;
using MyWeb.Domain.Entities.Payments;
using MyWeb.Repositories.Payments;

namespace MyWeb.Infrastructure.Repositories.Payments;

public class PaymentRepository : EfRepositoryBase<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class ShipmentRepository : EfRepositoryBase<Shipment>, IShipmentRepository
{
    public ShipmentRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class RefundRepository : EfRepositoryBase<Refund>, IRefundRepository
{
    public RefundRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
