using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rtl.Core.Application.Identity;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Entities;

namespace Rtl.Core.Infrastructure.Auditing.Interceptors;

/// <summary>
/// EF Core interceptor that converts hard deletes to soft deletes for entities
/// implementing ISoftDeletable, and populates soft delete audit fields.
/// </summary>
/// <remarks>
/// When an entity implementing ISoftDeletable is marked for deletion,
/// this interceptor converts the delete operation to an update that sets
/// IsDeleted = true along with DeletedAtUtc and DeletedByUserId.
/// </remarks>
public sealed class SoftDeleteInterceptor(
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            HandleSoftDeletes(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            HandleSoftDeletes(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private void HandleSoftDeletes(DbContext context)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var userId = currentUserService.UserId ?? Guid.Empty;

        var entries = context.ChangeTracker
            .Entries<SoftDeletableEntity>()
            .Where(e => e.State == EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            // Convert delete to update
            entry.State = EntityState.Modified;

            // Perform soft delete
            entry.Entity.IsDeleted = true;
            entry.Entity.DeletedAtUtc = utcNow;
            entry.Entity.DeletedByUserId = userId;
        }
    }
}
