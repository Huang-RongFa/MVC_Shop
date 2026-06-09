namespace MyWeb.Application.DTOs.Common;

public sealed class ApiResponse<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public ApiError? Error { get; init; }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string code, string message)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Error = new ApiError(code, message)
        };
    }
}

public sealed class ApiResponse
{
    public static ApiResponse<object> Success()
    {
        return ApiResponse<object>.Success(new { });
    }

    public static ApiResponse<object> Failure(string code, string message)
    {
        return ApiResponse<object>.Failure(code, message);
    }
}

public sealed record ApiError(string Code, string Message);
