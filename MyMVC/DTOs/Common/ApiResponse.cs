namespace MyWeb.DTOs.Common;

public class ApiResponse<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public ErrorResponse? Error { get; init; }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(ErrorCode code, string message)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Error = new ErrorResponse(code, message)
        };
    }
}

public class ApiResponse
{
    public bool Succeeded { get; init; }
    public ErrorResponse? Error { get; init; }

    public static ApiResponse Success()
    {
        return new ApiResponse
        {
            Succeeded = true
        };
    }

    public static ApiResponse Failure(ErrorCode code, string message)
    {
        return new ApiResponse
        {
            Succeeded = false,
            Error = new ErrorResponse(code, message)
        };
    }
}
