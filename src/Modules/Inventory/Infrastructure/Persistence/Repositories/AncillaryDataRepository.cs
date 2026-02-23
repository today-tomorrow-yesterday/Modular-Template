using Microsoft.EntityFrameworkCore;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class AncillaryDataRepository(InventoryDbContext dbContext)
    : CacheReadRepository<Domain.AncillaryData.AncillaryData, int, InventoryDbContext>(dbContext),
      Domain.AncillaryData.IAncillaryDataRepository
{
    public async Task<IReadOnlyCollection<Domain.AncillaryData.AncillaryData>> GetByHomeCenterAndStockNumbersAsync(
        int homeCenterNumber,
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(a => a.RefHomeCenterNumber == homeCenterNumber && stockNumbers.Contains(a.RefStockNumber))
            .ToListAsync(cancellationToken);
    }
}
