using Microsoft.EntityFrameworkCore;
using Rtl.Core.Application.Auditing;

namespace Rtl.Core.Infrastructure.Auditing;

internal sealed class AuditLogRepository<TContext>(TContext context) : IAuditLogRepository
    where TContext : DbContext
{
    public async Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityName,
        string entityId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<AuditLog>()
            .Where(a => a.EntityName == entityName && a.EntityId == entityId)
            .OrderByDescending(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<AuditLog>()
            .Where(a => a.CorrelationId == correlationId)
            .OrderBy(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetByUserAsync(
        Guid userId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<AuditLog>()
            .Where(a => a.UserId == userId &&
                        a.TimestampUtc >= fromUtc &&
                        a.TimestampUtc <= toUtc)
            .OrderByDescending(a => a.TimestampUtc)
            .Select(a => ToDto(a))
            .ToListAsync(cancellationToken);
    }

    private static AuditLogDto ToDto(AuditLog a) => new(
        a.Id,
        a.EntityName,
        a.EntityId,
        a.Action.ToString(),
        a.OldValues,
        a.NewValues,
        a.AffectedColumns,
        a.UserId,
        a.UserName,
        a.TimestampUtc,
        a.CorrelationId);
}
