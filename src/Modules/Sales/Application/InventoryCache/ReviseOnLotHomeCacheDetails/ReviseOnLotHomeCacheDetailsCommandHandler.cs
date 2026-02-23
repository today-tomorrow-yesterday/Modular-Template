using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCacheDetails;

// Flow: Inventory.OnLotHomeDetailsRevised → Sales.ReviseOnLotHomeCacheDetailsCommand → upsert cache.on_lot_homes
// Catch-all for non-price changes (model, make, bedrooms, etc.). Full ECST payload.
internal sealed class ReviseOnLotHomeCacheDetailsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IOnLotHomeCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReviseOnLotHomeCacheDetailsCommand>
{
    public async Task<Result> Handle(
        ReviseOnLotHomeCacheDetailsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
