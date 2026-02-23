using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain.SaleSummariesCache;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class SaleSummaryCacheRepository(InventoryDbContext dbContext)
    : CacheReadRepository<SaleSummaryCache, int, InventoryDbContext>(dbContext),
      ISaleSummaryCacheRepository,
      ISaleSummaryCacheWriter
{
    public async Task<IReadOnlyCollection<SaleSummaryCache>> GetByStockNumbersAsync(
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(s => stockNumbers.Contains(s.RefStockNumber))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertAsync(SaleSummaryCache cache, CancellationToken cancellationToken = default)
    {
        var existing = await DbSet.FindAsync([cache.Id], cancellationToken);

        if (existing is null)
        {
            DbSet.Add(cache);
        }
        else
        {
            existing.RefStockNumber = cache.RefStockNumber;
            existing.SaleId = cache.SaleId;
            existing.CustomerName = cache.CustomerName;
            existing.ReceivedInDate = cache.ReceivedInDate;
            existing.OriginalRetailPrice = cache.OriginalRetailPrice;
            existing.CurrentRetailPrice = cache.CurrentRetailPrice;
            existing.LastSyncedAtUtc = cache.LastSyncedAtUtc;
        }

        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
