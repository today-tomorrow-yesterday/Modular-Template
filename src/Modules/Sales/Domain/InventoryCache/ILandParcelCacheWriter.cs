namespace Modules.Sales.Domain.InventoryCache;

public interface ILandParcelCacheWriter
{
    Task UpsertAsync(LandParcelCache cache, CancellationToken cancellationToken = default);

    Task MarkAsRemovedByRefIdAsync(int refLandParcelId, CancellationToken cancellationToken = default);
}
