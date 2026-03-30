namespace Modules.Sales.Domain.InventoryCache;

public interface IInventoryCacheQueries
{
    // Resolve OnLot cache entry by home center + stock number (for HomeLine FK resolution)
    Task<OnLotHomeCache?> FindByHomeCenterAndStockAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default);

    // Resolve LandParcel cache entry by home center + stock number (for LandLine FK resolution)
    Task<LandParcelCache?> FindLandParcelByHomeCenterAndStockAsync(
        int homeCenterNumber,
        string stockNumber,
        CancellationToken cancellationToken = default);

    // Follow FK backwards: find all HomeLine package lines referencing this cache entry
    Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForHomeAsync(
        int onLotHomeCacheId,
        CancellationToken cancellationToken = default);

    // Follow FK backwards: find all LandLine package lines referencing this cache entry
    Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForLandAsync(
        int landParcelCacheId,
        CancellationToken cancellationToken = default);

    // Same as above but by Inventory's public ID (used during removal before cache row is deleted)
    Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForHomeByPublicIdAsync(
        Guid publicOnLotHomeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AffectedPackageLine>> GetPackageLinesForLandByPublicIdAsync(
        Guid publicLandParcelId,
        CancellationToken cancellationToken = default);
}

// Lightweight projection — just the IDs we need to locate affected packages
public sealed record AffectedPackageLine(int PackageLineId, int PackageId, int SaleId);
