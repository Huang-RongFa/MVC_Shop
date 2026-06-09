using MyWeb.Application.Interfaces.Pages;
using MyWeb.Models.ViewModels.Auth;

namespace MyWeb.Application.Services.Pages;

public sealed class AuthPageService : IAuthPageService
{
    public AuthLoginPageViewModel GetStorefrontLoginPage(string? returnUrl = null)
    {
        return new AuthLoginPageViewModel
        {
            Title = "會員登入",
            Subtitle = "登入後可使用購物車、結帳與我的訂單。",
            SubmitText = "登入會員",
            PostLoginPath = string.IsNullOrWhiteSpace(returnUrl) ? "/products" : returnUrl,
            IsAdminLogin = false
        };
    }

    public AuthLoginPageViewModel GetAdminLoginPage()
    {
        return new AuthLoginPageViewModel
        {
            Title = "後台管理登入",
            Subtitle = "請使用 Admin 或 Staff 帳號登入後台。",
            AccountLabel = "管理帳號 / Email",
            SubmitText = "登入後台",
            PostLoginPath = "/admin",
            IsAdminLogin = true
        };
    }

    public RegisterPageViewModel GetRegisterPage()
    {
        return new RegisterPageViewModel();
    }

    public ForgotPasswordPageViewModel GetForgotPasswordPage()
    {
        return new ForgotPasswordPageViewModel();
    }
}
