using MyWeb.Data;
using MyWeb.Domain.Entities.Accounts;
using MyWeb.Repositories.Accounts;
using Microsoft.EntityFrameworkCore;

namespace MyWeb.Infrastructure.Repositories.Accounts;

public class UserRepository : EfRepositoryBase<User>, IUserRepository
{
    public UserRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }

    public Task<User?> GetByAccountForLoginAsync(
        string account,
        CancellationToken cancellationToken = default)
    {
        return Set
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(
                user => user.Account == account && !user.IsDeleted,
                cancellationToken);
    }

    public Task<User?> GetByIdForClaimsAsync(
        long userId,
        CancellationToken cancellationToken = default)
    {
        return Set
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(
                user => user.UserId == userId && !user.IsDeleted,
                cancellationToken);
    }

    public void AddLoginLog(UserLoginLog loginLog)
    {
        DbContext.UserLoginLogs.Add(loginLog);
    }
}

public class RoleRepository : EfRepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}

public class PermissionRepository : EfRepositoryBase<Permission>, IPermissionRepository
{
    public PermissionRepository(AppDbContext dbContext)
        : base(dbContext)
    {
    }
}
