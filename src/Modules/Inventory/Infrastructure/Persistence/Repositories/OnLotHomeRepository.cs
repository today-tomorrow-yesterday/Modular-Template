using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain.OnLotHomes;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class OnLotHomeRepository(InventoryDbContext dbContext)
    : Repository<OnLotHome, int, InventoryDbContext>(dbContext),
      IOnLotHomeRepository
{
    protected override Expression<Func<OnLotHome, int>> IdSelector => entity => entity.Id;

    public override async Task<OnLotHome?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<OnLotHome?> GetByHomeCenterAndStockNumberAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                h => h.RefHomeCenterNumber == homeCenterNumber && h.RefStockNumber == stockNumber,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<OnLotHome>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(h => h.RefHomeCenterNumber == homeCenterNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OnLotHome>> GetByDimensionsAsync(
        decimal length,
        decimal width,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(h => h.Length == length && h.Width == width)
            .ToListAsync(cancellationToken);
    }
}
