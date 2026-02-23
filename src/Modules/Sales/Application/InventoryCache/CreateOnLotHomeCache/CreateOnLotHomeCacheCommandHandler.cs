using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.CreateOnLotHomeCache;

// Flow: Inventory.OnLotHomeAddedToInventory → Sales.CreateOnLotHomeCacheCommand → upsert cache.on_lot_homes
// Uses upsert for idempotency (duplicate events from EventBridge are safe).
internal sealed class CreateOnLotHomeCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IOnLotHomeCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateOnLotHomeCacheCommand>
{
    public async Task<Result> Handle(
        CreateOnLotHomeCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
