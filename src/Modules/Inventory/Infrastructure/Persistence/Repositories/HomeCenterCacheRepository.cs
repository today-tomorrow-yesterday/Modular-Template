using Modules.Inventory.Domain.HomeCentersCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class HomeCenterCacheRepository(InventoryDbContext dbContext)
    : CacheReadRepository<HomeCenterCache, int, InventoryDbContext>(dbContext),
      IHomeCenterCacheRepository,
      IHomeCenterCacheWriter
{
    public async Task UpsertAsync(HomeCenterCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet.FindAsync([cache.Id], cancellationToken);

        if (existing is null)
        {
            DbSet.Add(cache);
        }
        else
        {
            existing.RefHomeCenterNumber = cache.RefHomeCenterNumber;
            existing.LotName = cache.LotName;
            existing.StateCode = cache.StateCode;
            existing.Latitude = cache.Latitude;
            existing.Longitude = cache.Longitude;
            existing.ZoneId = cache.ZoneId;
            existing.RegionId = cache.RegionId;
            existing.LastSyncedAtUtc = cache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
