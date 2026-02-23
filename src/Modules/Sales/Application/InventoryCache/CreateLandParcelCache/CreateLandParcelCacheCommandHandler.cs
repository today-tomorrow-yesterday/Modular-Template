using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.CreateLandParcelCache;

// Flow: Inventory.LandParcelAddedToInventory → Sales.CreateLandParcelCacheCommand → upsert cache.land_parcels
// Uses upsert for idempotency (duplicate events from EventBridge are safe).
internal sealed class CreateLandParcelCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ILandParcelCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateLandParcelCacheCommand>
{
    public async Task<Result> Handle(
        CreateLandParcelCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
