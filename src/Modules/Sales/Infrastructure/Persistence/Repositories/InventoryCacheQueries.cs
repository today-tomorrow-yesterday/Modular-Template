using Microsoft.EntityFrameworkCore;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Packages.Land;

namespace Modules.Sales.Infrastructure.Persistence.Repositories;

internal sealed class InventoryCacheQueries(SalesDbContext dbContext) : IInventoryCacheQueries
{
    public async Task<OnLotHomeCache?> FindByHomeCenterAndStockAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.OnLotHomesCache
            .FirstOrDefaultAsync(
                c => c.RefHomeCenterNumber == homeCenterNumber && c.RefStockNumber == stockNumber,
                cancellationToken);
    }

    public async Task<LandParcelCache?> FindLandParcelByHomeCenterAndStockAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<LandParcelCache>()
            .FirstOrDefaultAsync(
                c => c.RefHomeCenterNumber == homeCenterNumber && c.RefStockNumber == stockNumber,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForHomeAsync(
        int onLotHomeCacheId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PackageLines
            .OfType<HomeLine>()
            .Where(hl => hl.OnLotHomeId == onLotHomeCacheId)
            .Select(hl => new AffectedPackageLine(hl.Id, hl.PackageId, hl.Package.SaleId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForLandAsync(
        int landParcelCacheId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PackageLines
            .OfType<LandLine>()
            .Where(ll => ll.LandParcelId == landParcelCacheId)
            .Select(ll => new AffectedPackageLine(ll.Id, ll.PackageId, ll.Package.SaleId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForHomeByRefIdAsync(
        int refOnLotHomeId,
        CancellationToken cancellationToken = default)
    {
        // Join through cache table to find affected package lines by Inventory's ref ID
        return await dbContext.Set<OnLotHomeCache>()
            .Where(c => c.RefOnLotHomeId == refOnLotHomeId)
            .Join(
                dbContext.PackageLines.OfType<HomeLine>(),
                cache => cache.Id,
                hl => hl.OnLotHomeId,
                (cache, hl) => new AffectedPackageLine(hl.Id, hl.PackageId, hl.Package.SaleId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForLandByRefIdAsync(
        int refLandParcelId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<LandParcelCache>()
            .Where(c => c.RefLandParcelId == refLandParcelId)
            .Join(
                dbContext.PackageLines.OfType<LandLine>(),
                cache => cache.Id,
                ll => ll.LandParcelId,
                (cache, ll) => new AffectedPackageLine(ll.Id, ll.PackageId, ll.Package.SaleId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
