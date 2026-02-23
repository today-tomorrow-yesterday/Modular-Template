using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;

// Flow: Inventory.LandParcelRemovedFromInventory → Sales.RemoveLandParcelCacheCommand → delete cache.land_parcels row
// DeleteBehavior.SetNull on FK ensures package_lines.land_parcel_id → NULL (no cascade delete).
//
// PRODUCT REMOVAL vs PRODUCT CLAIMED — two distinct flows use the same 1:many FK query:
//
// Flow 1 (this handler): Inventory removes a land parcel from its catalog.
//   Trigger: LandParcelRemovedFromInventory (process trigger event from Inventory)
//   Meaning: The land parcel no longer exists in the iSeries inventory feed.
//   Response: Delete cache row, FK goes NULL. Affected packages lose their product reference
//   but retain all data in LandDetails JSONB. Salesperson sees the land is no longer in inventory.
//
// Flow 2 (not yet implemented): Sales claims a land parcel for a specific sale.
//   Trigger: TBD — same open question as OnLotHome (see RemoveOnLotHomeCacheCommandHandler).
//   Response: The claiming sale keeps its FK. All OTHER packages referencing the same land
//   should have their FK cleared and be flagged/notified.
//   After claiming, Sales publishes SaleSummaryChanged → Inventory receives SaleId.
//
// OPEN QUESTION: What Sales command represents "this land is now claimed"?
//   Same answer as homes — once the business defines the claim trigger, both home and land
//   use the same pattern: query the 1:many FK, clear competing references, notify.
internal sealed class RemoveLandParcelCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    ILandParcelCacheWriter cacheWriter,
    IInventoryCacheQueries cacheQueries,
    ILogger<RemoveLandParcelCacheCommandHandler> logger)
    : ICommandHandler<RemoveLandParcelCacheCommand>
{
    public async Task<Result> Handle(
        RemoveLandParcelCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var affectedLines = await cacheQueries.GetPackageLinesForLandByRefIdAsync(
            request.RefLandParcelId,
            cancellationToken);

        if (affectedLines.Count > 0)
        {
            logger.LogWarning(
                "LandParcel {StockNumber} (HC={HomeCenterNumber}) removed from inventory — {Count} package line(s) will lose product reference: {Affected}",
                request.StockNumber,
                request.HomeCenterNumber,
                affectedLines.Count,
                string.Join(", ", affectedLines.Select(l => $"Sale={l.SaleId}/Pkg={l.PackageId}/Line={l.PackageLineId}")));

            // TODO: For each affected package, dispatch a command to flag the LandLine
            // as "product no longer in inventory." The salesperson should see a warning
            // and can either select a different land parcel or convert to manual entry.
        }

        await cacheWriter.RemoveByRefIdAsync(request.RefLandParcelId, cancellationToken);

        return Result.Success();
    }
}
