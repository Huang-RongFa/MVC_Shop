namespace MyWeb.DTOs.Common;

public enum ErrorCode
{
    None = 0,
    ValidationFailed = 1000,
    Unauthorized = 1001,
    Forbidden = 1002,
    NotFound = 1003,
    Conflict = 1004,
    BusinessRuleViolation = 1005,
    TooManyRequests = 1006,
    InternalServerError = 9000,
    NotImplemented = 9001
}
