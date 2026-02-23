using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheAppraisal;

// Flow: Inventory.LandParcelAppraisalRevised → Sales.ReviseLandParcelCacheAppraisalCommand → upsert cache.land_parcels → raises LandParcelCacheAppraisalRevised
// Full ECST payload — all properties updated, not just appraisal fields.
internal sealed class ReviseLandParcelCacheAppraisalCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ILandParcelCacheWriter cacheWriter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<ReviseLandParcelCacheAppraisalCommand>
{
    public async Task<Result> Handle(
        ReviseLandParcelCacheAppraisalCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        request.Cache.LastSyncedAtUtc = dateTimeProvider.UtcNow;

        await cacheWriter.UpsertAsync(request.Cache, cancellationToken);

        return Result.Success();
    }
}
