namespace MyWeb.Application.Exceptions;

public sealed class AdminSystemValidationException : InvalidOperationException
{
    public AdminSystemValidationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
