using Microsoft.EntityFrameworkCore;
using MyWeb.Data;

namespace MyWeb.Infrastructure.Repositories;

public abstract class EfRepositoryBase<TEntity>
    where TEntity : class
{
    protected EfRepositoryBase(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    protected AppDbContext DbContext { get; }

    protected DbSet<TEntity> Set => DbContext.Set<TEntity>();
}
