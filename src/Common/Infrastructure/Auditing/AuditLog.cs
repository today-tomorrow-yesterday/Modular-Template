namespace ModularTemplate.Infrastructure.Auditing;

/// <summary>
/// Represents a single audit log entry capturing entity changes.
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; init; }

    // Entity identification
    public required string EntityName { get; init; }
    public required string EntityId { get; init; }
    public required AuditAction Action { get; init; }

    // Change data (JSON)
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public string? AffectedColumns { get; init; }

    // User context
    public required Guid UserId { get; init; }
    public string? UserName { get; init; }

    // Timing
    public required DateTime TimestampUtc { get; init; }

    // Request context
    public string? CorrelationId { get; init; }
    public string? TraceId { get; init; }
    public string? UserAgent { get; init; }
}

public enum AuditAction
{
    Unknown = 0,
    Insert = 1,
    Update = 2,
    Delete = 3,
    SoftDelete = 4,
    Restore = 5
}
