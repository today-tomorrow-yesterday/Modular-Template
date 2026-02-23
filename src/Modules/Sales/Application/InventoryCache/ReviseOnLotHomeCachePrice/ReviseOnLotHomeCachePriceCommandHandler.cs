using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCachePrice;

// Flow: Inventory.OnLotHomePriceRevised → Sales.ReviseOnLotHomeCachePriceCommand → upsert cache.on_lot_homes → raises OnLotHomeCachePriceRevised
// Full ECST payload — all properties updated, not just price fields.
internal sealed class ReviseOnLotHomeCachePriceCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IOnLotHomeCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReviseOnLotHomeCachePriceCommand>
{
    public async Task<Result> Handle(
        ReviseOnLotHomeCachePriceCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
