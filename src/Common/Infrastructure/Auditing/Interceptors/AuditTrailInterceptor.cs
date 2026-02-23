using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rtl.Core.Application.Auditing;
using Rtl.Core.Application.Identity;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Infrastructure.Security;

namespace Rtl.Core.Infrastructure.Auditing.Interceptors;

/// <summary>
/// EF Core interceptor that captures field-level changes and writes audit logs.
/// Encrypts sensitive data in the audit log (stores ciphertext).
/// </summary>
public sealed class AuditTrailInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider,
    IAuditContext auditContext,
    IEncryptionService encryptionService) : SaveChangesInterceptor
{
    private List<AuditEntry> _tempEntries = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        var auditEntries = CreateAuditEntries(eventData.Context);

        if (auditEntries.Count is 0)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Store entries without temporary properties immediately
        foreach (var entry in auditEntries.Where(e => !e.HasTemporaryProperties))
        {
            eventData.Context.Set<AuditLog>().Add(entry.ToAuditLog());
        }

        // Keep temp entries for SavedChanges
        if (auditEntries.Any(e => e.HasTemporaryProperties))
        {
            eventData.Context.ChangeTracker.AutoDetectChangesEnabled = false;
            _tempEntries = auditEntries.Where(e => e.HasTemporaryProperties).ToList();
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_tempEntries.Count > 0 && eventData.Context is not null)
        {
            foreach (var auditEntry in _tempEntries)
            {
                // Get the generated ID
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.SetEntityId(prop.CurrentValue?.ToString() ?? "");
                    }
                }

                eventData.Context.Set<AuditLog>().Add(auditEntry.ToAuditLog());
            }

            _tempEntries.Clear();
            await eventData.Context.SaveChangesAsync(cancellationToken);
            eventData.Context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is null)
            return base.SavingChanges(eventData, result);

        var auditEntries = CreateAuditEntries(eventData.Context);

        foreach (var entry in auditEntries.Where(e => !e.HasTemporaryProperties))
        {
            eventData.Context.Set<AuditLog>().Add(entry.ToAuditLog());
        }

        return base.SavingChanges(eventData, result);
    }

    private List<AuditEntry> CreateAuditEntries(DbContext context)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var userId = currentUserService.UserId ?? Guid.Empty;
        var auditEntries = new List<AuditEntry>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditable || entry.Entity is AuditLog || entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            var auditEntry = new AuditEntry(entry)
            {
                UserId = userId,
                UserName = auditContext.UserName,
                TimestampUtc = utcNow,
                CorrelationId = auditContext.CorrelationId,
                TraceId = auditContext.TraceId,
                UserAgent = auditContext.UserAgent,
                Action = DetermineAction(entry)
            };

            foreach (var property in entry.Properties)
            {
                if (property.Metadata.IsShadowProperty() && !property.Metadata.IsForeignKey())
                    continue;

                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    if (property.IsTemporary)
                        auditEntry.TemporaryProperties.Add(property);
                    else
                        auditEntry.SetEntityId(property.CurrentValue?.ToString() ?? "");
                    continue;
                }

                // Handle Sensitive Data (Encrypt for Log)
                var isSensitive = SensitiveDataCache.IsSensitive(entry.Entity.GetType(), propertyName);

                var oldValue = isSensitive 
                    ? encryptionService.Encrypt(property.OriginalValue?.ToString()) 
                    : property.OriginalValue;

                var newValue = isSensitive 
                    ? encryptionService.Encrypt(property.CurrentValue?.ToString()) 
                    : property.CurrentValue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = newValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = oldValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = oldValue;
                            auditEntry.NewValues[propertyName] = newValue;
                            auditEntry.AffectedColumns.Add(propertyName);
                        }
                        break;
                }
            }

            if (auditEntry.OldValues.Count > 0 || auditEntry.NewValues.Count > 0 || entry.State == EntityState.Deleted)
            {
                auditEntries.Add(auditEntry);
            }
        }

        return auditEntries;
    }

    private static AuditAction DetermineAction(EntityEntry entry)
    {
        if (entry.State == EntityState.Added) return AuditAction.Insert;
        if (entry.State == EntityState.Deleted) return AuditAction.Delete;
        
        if (entry.State == EntityState.Modified)
        {
            var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase));
            if (isDeletedProp is not null)
            {
                var wasDeleted = isDeletedProp.OriginalValue as bool? ?? false;
                var isDeleted = isDeletedProp.CurrentValue as bool? ?? false;
                if (!wasDeleted && isDeleted) return AuditAction.SoftDelete;
                if (wasDeleted && !isDeleted) return AuditAction.Restore;
            }
            return AuditAction.Update;
        }

        return AuditAction.Unknown;
    }
}