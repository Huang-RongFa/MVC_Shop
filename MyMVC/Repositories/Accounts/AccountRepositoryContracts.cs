using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Repositories.Accounts;

public interface IUserRepository
{
    Task<User?> GetByAccountForLoginAsync(
        string account,
        CancellationToken cancellationToken = default);

    Task<User?> GetByIdForClaimsAsync(
        long userId,
        CancellationToken cancellationToken = default);

    void AddLoginLog(UserLoginLog loginLog);
}

public interface IRoleRepository
{
}

public interface IPermissionRepository
{
}
