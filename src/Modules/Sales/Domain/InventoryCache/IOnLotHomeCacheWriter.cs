namespace Modules.Sales.Domain.InventoryCache;

public interface IOnLotHomeCacheWriter
{
    Task UpsertAsync(OnLotHomeCache cache, CancellationToken cancellationToken = default);

    Task MarkAsRemovedByPublicIdAsync(Guid publicOnLotHomeId, CancellationToken cancellationToken = default);
}
