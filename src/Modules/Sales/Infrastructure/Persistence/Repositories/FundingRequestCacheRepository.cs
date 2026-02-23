using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.FundingCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class FundingRequestCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<FundingRequestCache, int, SalesDbContext>(dbContext),
      IFundingRequestCacheRepository,
      IFundingRequestCacheWriter
{
    public async Task<FundingRequestCache?> GetByPackageIdAsync(int packageId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.PackageId == packageId, cancellationToken);
    }

    public async Task UpsertAsync(FundingRequestCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet
            .FirstOrDefaultAsync(f => f.RefFundingRequestId == cache.RefFundingRequestId, cancellationToken);

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
}
