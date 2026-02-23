using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.Cdc;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class CdcProjectCostQueries(SalesDbContext dbContext) : ICdcProjectCostQueries
{
    private const int MasterDealer = 29;

    public async Task<IReadOnlyCollection<CdcProjectCostCategory>> GetCategoriesWithItemsAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CdcProjectCostCategories
            .Where(c => c.MasterDealer == MasterDealer)
            .Include(c => c.Items.Where(i => i.MasterDealer == MasterDealer))
            .OrderBy(c => c.CategoryNumber)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<CdcProjectCostStateMatrix>> GetStateMatrixAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.CdcProjectCostStateMatrices
            .Where(m => m.MasterDealer == MasterDealer)
            .OrderBy(m => m.CategoryId)
            .ThenBy(m => m.CategoryItemId)
            .ThenBy(m => m.StateCode)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
