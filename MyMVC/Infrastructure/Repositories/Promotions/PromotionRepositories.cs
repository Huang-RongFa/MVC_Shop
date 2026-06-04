using MyWeb.Data;
using MyWeb.Domain.Entities.Promotions;
using MyWeb.Repositories.Promotions;

namespace MyWeb.Infrastructure.Repositories.Promotions;

public class CouponRepository : EfRepositoryBase<Coupon>, ICouponRepository
{
    public CouponRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class PromotionRepository : EfRepositoryBase<Promotion>, IPromotionRepository
{
    public PromotionRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
