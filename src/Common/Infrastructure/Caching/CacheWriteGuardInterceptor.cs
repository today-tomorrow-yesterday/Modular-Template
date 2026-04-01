using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ModularTemplate.Application.Caching;
using ModularTemplate.Domain.Caching;

namespace ModularTemplate.Infrastructure.Caching;

/// <summary>
/// EF Core interceptor that prevents accidental persistence of cache projection entities.
/// Cache entities can only be saved when an explicit write scope is active.
/// </summary>
public sealed class CacheWriteGuardInterceptor(ICacheWriteScope cacheWriteScope) : SaveChangesInterceptor
{
    private readonly ICacheWriteScope _cacheWriteScope = cacheWriteScope;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            GuardCacheWrites(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            GuardCacheWrites(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void GuardCacheWrites(DbContext context)
    {
        if (_cacheWriteScope.IsWriteAllowed)
        {
            return;
        }

        var cacheEntities = context.ChangeTracker
            .Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Entity is ICacheProjection)
            .Select(e => e.Entity.GetType().Name)
            .Distinct()
            .ToList();

        if (cacheEntities.Count > 0)
        {
            throw new InvalidOperationException(
                $"Attempted to modify cache projection entities [{string.Join(", ", cacheEntities)}] " +
                "outside of an authorized write scope. Cache entities can only be modified within " +
                "integration event handlers using ICacheWriteScope.AllowWrites().");
        }
    }
}
