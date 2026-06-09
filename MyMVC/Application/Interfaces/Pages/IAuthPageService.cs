using MyWeb.Models.ViewModels.Auth;

namespace MyWeb.Application.Interfaces.Pages;

public interface IAuthPageService
{
    AuthLoginPageViewModel GetStorefrontLoginPage(string? returnUrl = null);

    AuthLoginPageViewModel GetAdminLoginPage();

    RegisterPageViewModel GetRegisterPage();

    ForgotPasswordPageViewModel GetForgotPasswordPage();
}
