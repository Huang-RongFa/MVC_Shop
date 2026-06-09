using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Application.Interfaces.Auth;

/// <summary>
/// 登入流程需要的使用者資料存取介面。
/// Repository 不負責判斷登入是否成功，只提供 Service 驗證所需資料與紀錄寫入。
/// </summary>
public interface IUserAuthRepository
{
    /// <summary>查詢可登入前台的 Customer 候選帳號；不可回傳敏感欄位到 Controller 或 View。</summary>
    Task<User?> GetStorefrontCustomerByAccountAsync(string account, CancellationToken cancellationToken);

    /// <summary>查詢可登入後台的 Admin / Staff 候選帳號與必要角色權限關聯。</summary>
    Task<User?> GetAdminCandidateByAccountAsync(string account, CancellationToken cancellationToken);

    /// <summary>寫入登入成功或失敗紀錄；不可記錄明文密碼、Cookie 或 Token。</summary>
    Task RecordLoginAsync(
        long? userId,
        string account,
        string loginResult,
        string? failureReason,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken);
}
