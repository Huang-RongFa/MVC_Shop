namespace MyWeb.Models.ViewModels.Auth;

public sealed class AuthLoginPageViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Subtitle { get; init; } = string.Empty;

    public string AccountLabel { get; init; } = "Email 或帳號";

    public string SubmitText { get; init; } = "登入";

    public string PostLoginPath { get; init; } = "/";

    public bool IsAdminLogin { get; init; }
}

public sealed class RegisterPageViewModel
{
    public string Title { get; init; } = "建立會員帳號";

    public string Subtitle { get; init; } = "註冊後即可使用購物車、結帳與訂單查詢。";
}

public sealed class ForgotPasswordPageViewModel
{
    public string Title { get; init; } = "忘記密碼";

    public string Subtitle { get; init; } = "輸入 Email 後，系統會寄送短效重設連結。";
}

public sealed class MemberProfileViewModel
{
    public string DisplayName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string PhoneNumber { get; init; } = string.Empty;

    public string DefaultShippingAddress { get; init; } = string.Empty;
}
