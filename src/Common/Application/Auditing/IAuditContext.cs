namespace Rtl.Core.Application.Auditing;

/// <summary>
/// Provides contextual information for audit log entries.
/// </summary>
public interface IAuditContext
{
    string? UserName { get; }
    string? CorrelationId { get; }
    string? TraceId { get; }
    string? UserAgent { get; }
}
