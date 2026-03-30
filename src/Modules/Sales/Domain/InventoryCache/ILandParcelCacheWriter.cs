namespace Modules.Sales.Domain.InventoryCache;

public interface ILandParcelCacheWriter
{
    Task UpsertAsync(LandParcelCache cache, CancellationToken cancellationToken = default);

    Task MarkAsRemovedByPublicIdAsync(Guid publicLandParcelId, CancellationToken cancellationToken = default);
}
