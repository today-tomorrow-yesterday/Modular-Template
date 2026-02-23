using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Rtl.Core.Infrastructure.Auditing;

/// <summary>
/// Builder class for constructing AuditLog entries from EF Core change tracking.
/// </summary>
internal sealed class AuditEntry(EntityEntry entry)
{
    public EntityEntry Entry { get; } = entry;
    public string EntityName { get; } = entry.Entity.GetType().Name;
    public string EntityId { get; private set; } = string.Empty;
    public AuditAction Action { get; set; }
    public Dictionary<string, object?> OldValues { get; } = [];
    public Dictionary<string, object?> NewValues { get; } = [];
    public List<string> AffectedColumns { get; } = [];

    // Context (set externally)
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? CorrelationId { get; set; }
    public string? TraceId { get; set; }
    public string? UserAgent { get; set; }

    // For handling temporary keys (e.g., identity columns)
    public List<PropertyEntry> TemporaryProperties { get; } = [];
    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;

    public void SetEntityId(string entityId) => EntityId = entityId;

    public AuditLog ToAuditLog()
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = EntityName,
            EntityId = EntityId,
            Action = Action,
            OldValues = OldValues.Count > 0
                ? JsonSerializer.Serialize(OldValues)
                : null,
            NewValues = NewValues.Count > 0
                ? JsonSerializer.Serialize(NewValues)
                : null,
            AffectedColumns = AffectedColumns.Count > 0
                ? JsonSerializer.Serialize(AffectedColumns)
                : null,
            UserId = UserId,
            UserName = UserName,
            TimestampUtc = TimestampUtc,
            CorrelationId = CorrelationId,
            TraceId = TraceId,
            UserAgent = UserAgent
        };
    }
}
