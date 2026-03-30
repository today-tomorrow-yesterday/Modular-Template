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
            .FirstOrDefaultAsync(l => l.RefPublicId == cache.RefPublicId, cancellationToken);

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

    public async Task MarkAsRemovedByPublicIdAsync(Guid publicLandParcelId, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(l => l.RefPublicId == publicLandParcelId, cancellationToken);

        if (existing is not null)
        {
            existing.MarkAsRemovedFromInventory();
            await DbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
