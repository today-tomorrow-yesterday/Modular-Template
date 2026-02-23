using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheDetails;

// Flow: Inventory.LandParcelDetailsRevised → Sales.ReviseLandParcelCacheDetailsCommand → upsert cache.land_parcels
// Catch-all for non-appraisal changes (address, county, stock type, etc.). Full ECST payload.
internal sealed class ReviseLandParcelCacheDetailsCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ILandParcelCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReviseLandParcelCacheDetailsCommand>
{
    public async Task<Result> Handle(
        ReviseLandParcelCacheDetailsCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
