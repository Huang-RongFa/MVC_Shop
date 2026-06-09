using Microsoft.EntityFrameworkCore;
using MyWeb.Application.Interfaces.Auth;
using MyWeb.Data;
using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Infrastructure.Repositories.Auth;

/// <summary>
/// 前台與後台登入所需的帳號資料存取。
/// Repository 僅查詢與寫入資料，不決定登入規則、權限規則或 Cookie 行為。
/// </summary>
public sealed class UserAuthRepository : IUserAuthRepository
{
    private readonly AppDbContext _dbContext;

    public UserAuthRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetStorefrontCustomerByAccountAsync(string account, CancellationToken cancellationToken)
    {
        // 前台登入只需驗證 Customer 身分與基本帳號狀態，不載入後台權限關聯，降低不必要資料讀取。
        return _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => (user.Account == account || user.Email == account)
                    && !user.IsDeleted
                    && user.UserType == "Customer",
                cancellationToken);
    }

    public Task<User?> GetAdminCandidateByAccountAsync(string account, CancellationToken cancellationToken)
    {
        // 登入流程需要 Role / Permission Claim，因此在單次查詢載入必要關聯；列表查詢不可照搬此 Include 深度。
        return _dbContext.Users
            .AsNoTracking()
            .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                    .ThenInclude(role => role.RolePermissions)
                        .ThenInclude(rolePermission => rolePermission.Permission)
            .FirstOrDefaultAsync(
                user => user.Account == account
                    && !user.IsDeleted
                    && (user.UserType == "Admin" || user.UserType == "Staff"),
                cancellationToken);
    }

    public async Task RecordLoginAsync(
        long? userId,
        string account,
        string loginResult,
        string? failureReason,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken)
    {
        // TODO: 正式環境可補充裝置指紋、風險等級與失敗次數統計，但不可記錄明文密碼或 Cookie / Token。
        _dbContext.UserLoginLogs.Add(new UserLoginLog
        {
            UserId = userId,
            Account = account,
            LoginResult = loginResult,
            FailureReason = failureReason,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LoginAt = DateTime.UtcNow
        });

        if (userId.HasValue && loginResult == "Success")
        {
            // 使用 ExecuteUpdateAsync 避免為更新 LastLoginAt 額外追蹤整個 User Entity。
            await _dbContext.Users
                .Where(user => user.UserId == userId.Value)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(user => user.LastLoginAt, DateTime.UtcNow),
                    cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
