using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.RetailLocationCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class RetailLocationCacheRepository(SalesDbContext dbContext)
    : CacheReadRepository<RetailLocationCache, int, SalesDbContext>(dbContext),
      IRetailLocationCacheRepository
{
    public void Add(RetailLocationCache entity)
    {
        DbSet.Add(entity);
    }

    public async Task<RetailLocationCache?> GetByHomeCenterNumberAsync(int homeCenterNumber, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(r => r.RefHomeCenterNumber == homeCenterNumber, cancellationToken);
    }
}
