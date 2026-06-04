using MyWeb.Data;
using MyWeb.Domain.Entities.Logs;
using MyWeb.Repositories.Logs;

namespace MyWeb.Infrastructure.Repositories.Logs;

public class AuditRepository : EfRepositoryBase<AuditLog>, IAuditRepository
{
    public AuditRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class LogRepository : EfRepositoryBase<SystemErrorLog>, ILogRepository
{
    public LogRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
