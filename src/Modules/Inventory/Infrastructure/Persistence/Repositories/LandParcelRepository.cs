using Microsoft.EntityFrameworkCore;
using Modules.Inventory.Domain.LandParcels;
using Rtl.Core.Infrastructure.Persistence;
using System.Linq.Expressions;

namespace Modules.Inventory.Infrastructure.Persistence.Repositories;

internal sealed class LandParcelRepository(InventoryDbContext dbContext)
    : Repository<LandParcel, int, InventoryDbContext>(dbContext),
      ILandParcelRepository
{
    protected override Expression<Func<LandParcel, int>> IdSelector => entity => entity.Id;

    public override async Task<LandParcel?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<LandParcel?> GetByHomeCenterAndStockNumberAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                l => l.RefHomeCenterNumber == homeCenterNumber && l.RefStockNumber == stockNumber,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<LandParcel>> GetByHomeCenterNumberAsync(
        int homeCenterNumber,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(l => l.RefHomeCenterNumber == homeCenterNumber)
            .ToListAsync(cancellationToken);
    }
}
