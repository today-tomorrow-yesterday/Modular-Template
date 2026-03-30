namespace Modules.Sales.Domain.InventoryCache;

public interface IOnLotHomeCacheWriter
{
    Task UpsertAsync(OnLotHomeCache cache, CancellationToken cancellationToken = default);

    Task MarkAsRemovedByRefIdAsync(int refOnLotHomeId, CancellationToken cancellationToken = default);
}
