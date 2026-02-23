using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain.WheelsAndAxles;
using Rtl.Core.Infrastructure.Caching;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class WheelsAndAxlesTransactionRepository(InventoryDbContext dbContext)
    : CacheReadRepository<WheelsAndAxlesTransaction, int, InventoryDbContext>(dbContext),
      IWheelsAndAxlesTransactionRepository
{
    public async Task<IReadOnlyCollection<WheelsAndAxlesTransaction>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(w => w.RefHomeCenterNumber == homeCenterNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<WheelsAndAxlesTransaction?> GetLatestByStockNumbersAsync(
        IReadOnlySet<string> stockNumbers,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(w => w.StockNumber != null && stockNumbers.Contains(w.StockNumber))
            .OrderByDescending(w => w.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
