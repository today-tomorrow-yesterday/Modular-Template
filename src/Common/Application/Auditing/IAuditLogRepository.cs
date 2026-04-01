namespace ModularTemplate.Application.Auditing;

/// <summary>
/// Repository for querying audit logs.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Gets audit history for a specific entity.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID (same HTTP request).
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for a specific user within a date range.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(
        Guid userId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}

public sealed record AuditLogDto(
    Guid Id,
    string EntityName,
    string EntityId,
    string Action,
    string? OldValues,
    string? NewValues,
    string? AffectedColumns,
    Guid UserId,
    string? UserName,
    DateTime TimestampUtc,
    string? CorrelationId);
