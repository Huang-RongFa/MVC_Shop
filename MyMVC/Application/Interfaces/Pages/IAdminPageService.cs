using System.Security.Claims;
using MyWeb.Models.ViewModels.Admin;

namespace MyWeb.Application.Interfaces.Pages;

/// <summary>
/// 後台 MVC 頁面資料組裝介面。
/// TODO: 正式實作需在 Service 層套用 Role / Permission 檢查，不可只靠 View 隱藏功能。
/// </summary>
public interface IAdminPageService
{
    /// <summary>取得後台 Dashboard；正式資料包含營收、訂單、出貨與庫存摘要。</summary>
    Task<AdminDashboardViewModel> GetDashboardAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得後台分析資料；此類敏感營運指標需限制 Analytics.Read 或等效權限。</summary>
    Task<AdminAnalyticsViewModel> GetAnalyticsAsync(ClaimsPrincipal user, CancellationToken cancellationToken);

    /// <summary>取得共用後台模組頁骨架；正式資料應由 moduleKey 對應的專責 Service 產生。</summary>
    Task<AdminModulePageViewModel> GetModulePageAsync(
        ClaimsPrincipal user,
        string moduleKey,
        CancellationToken cancellationToken);
}
