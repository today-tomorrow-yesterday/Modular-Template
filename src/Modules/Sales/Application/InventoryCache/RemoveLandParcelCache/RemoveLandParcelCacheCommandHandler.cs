using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Land;
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
    IPackageRepository packageRepository,
    ILogger<RemoveLandParcelCacheCommandHandler> logger)
    : ICommandHandler<RemoveLandParcelCacheCommand>
{
    public async Task<Result> Handle(
        RemoveLandParcelCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var affectedLines = await cacheQueries.GetPackageLinesForLandByPublicIdAsync(
            request.PublicLandParcelId,
            cancellationToken);

        if (affectedLines.Count > 0)
        {
            logger.LogWarning(
                "LandParcel {PublicLandParcelId} removed from inventory — {Count} package line(s) will lose product reference: {Affected}",
                request.PublicLandParcelId,
                affectedLines.Count,
                string.Join(", ", affectedLines.Select(l => $"Sale={l.SaleId}/Pkg={l.PackageId}/Line={l.PackageLineId}")));

            foreach (var affected in affectedLines)
            {
                var package = await packageRepository.GetByIdWithTrackingAsync(affected.PackageId, cancellationToken);
                if (package is null) continue;

                var landLine = package.Lines.OfType<LandLine>().FirstOrDefault(l => l.Id == affected.PackageLineId);
                if (landLine is null) continue;

                package.MarkLineProductUnavailable(landLine, "Land", landLine.Details?.LandStockNumber);
            }
        }

        await cacheWriter.MarkAsRemovedByPublicIdAsync(request.PublicLandParcelId, cancellationToken);

        return Result.Success();
    }
}
