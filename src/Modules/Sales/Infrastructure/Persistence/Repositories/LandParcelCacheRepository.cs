using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class LandParcelCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<LandParcelCache, SalesDbContext>(dbContext),
      ILandParcelCacheWriter
{
    public async Task UpsertAsync(LandParcelCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(l => l.RefLandParcelId == cache.RefLandParcelId, cancellationToken);

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

    public async Task MarkAsRemovedByRefIdAsync(int refLandParcelId, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(l => l.RefLandParcelId == refLandParcelId, cancellationToken);

        if (existing is not null)
        {
            existing.MarkAsRemovedFromInventory();
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
