using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class OnLotHomeCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<OnLotHomeCache, SalesDbContext>(dbContext),
      IOnLotHomeCacheWriter
{
    public async Task UpsertAsync(OnLotHomeCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(h => h.RefPublicId == cache.RefPublicId, cancellationToken);

        if (existing is null)
        {
            DbSet.Add(cache);
        }
        else
        {
            existing.ApplyChangesFrom(cache);
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAsRemovedByPublicIdAsync(Guid publicOnLotHomeId, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(h => h.RefPublicId == publicOnLotHomeId, cancellationToken);

        if (existing is not null)
        {
            existing.MarkAsRemovedFromInventory();
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
