namespace Modules.Sales.Domain.InventoryCache;

public interface ILandParcelCacheWriter
{
    Task UpsertAsync(LandParcelCache cache, CancellationToken cancellationToken = default);

    Task RemoveByRefIdAsync(int refLandParcelId, CancellationToken cancellationToken = default);
}
