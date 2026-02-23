using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain.LandCosts;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class LandCostRepository(InventoryDbContext dbContext)
    : CacheReadRepository<LandCost, int, InventoryDbContext>(dbContext),
      ILandCostRepository
{
    public async Task<IReadOnlyCollection<LandCost>> GetByHomeCenterAndStockNumbersAsync(
        int homeCenterNumber,
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(l => l.RefHomeCenterNumber == homeCenterNumber && stockNumbers.Contains(l.RefStockNumber))
            .ToListAsync(cancellationToken);
    }
}
