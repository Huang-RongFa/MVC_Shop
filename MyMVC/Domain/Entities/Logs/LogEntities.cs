using MyWeb.Domain.Entities.Accounts;

namespace MyWeb.Domain.Entities.Logs;

public class AuditLog
{
    public long AuditLogId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string RecordId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string? OldValueJson { get; set; }
    public string? NewValueJson { get; set; }
    public long? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? IpAddress { get; set; }

    public User? ChangedByUser { get; set; }
}

public class AdminActionLog
{
    public long AdminActionLogId { get; set; }
    public long UserId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public string? TargetId { get; set; }
    public string? Description { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}

public class SystemErrorLog
{
    public long ErrorLogId { get; set; }
    public string ErrorLevel { get; set; } = string.Empty;
    public string? Source { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public string? RequestBody { get; set; }
    public long? UserId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}
