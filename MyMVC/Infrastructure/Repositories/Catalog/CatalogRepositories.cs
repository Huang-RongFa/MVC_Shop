using MyWeb.Data;
using MyWeb.Domain.Entities.Catalog;
using MyWeb.Repositories.Catalog;

namespace MyWeb.Infrastructure.Repositories.Catalog;

public class ProductSkuRepository : EfRepositoryBase<ProductSku>, IProductSkuRepository
{
    public ProductSkuRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class ProductCategoryRepository : EfRepositoryBase<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
