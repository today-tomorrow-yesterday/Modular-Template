using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.InventoryCache.RemoveOnLotHomeCache;

// Flow: Inventory.OnLotHomeRemovedFromInventory → Sales.RemoveOnLotHomeCacheCommand → delete cache.on_lot_homes row
// DeleteBehavior.SetNull on FK ensures package_lines.on_lot_home_id → NULL (no cascade delete).
//
// PRODUCT REMOVAL vs PRODUCT CLAIMED — two distinct flows use the same 1:many FK query:
//
// Flow 1 (this handler): Inventory removes a product from its catalog.
//   Trigger: OnLotHomeRemovedFromInventory (process trigger event from Inventory)
//   Meaning: The home no longer exists in the iSeries inventory feed.
//   Response: Delete cache row, FK goes NULL. Affected packages lose their product reference
//   but retain all data in HomeDetails JSONB. Salesperson sees the home is no longer in inventory.
//
// Flow 2 (not yet implemented): Sales claims a product for a specific sale.
//   Trigger: TBD — the command/process where a sale "locks in" a home (e.g. funding submission?)
//   Meaning: This home is spoken for. Other packages should not reference it.
//   Response: The claiming sale keeps its FK. All OTHER packages referencing the same home
//   should have their FK cleared and be flagged/notified.
//   After claiming, Sales publishes SaleSummaryChanged → Inventory receives SaleId for the home.
//
// OPEN QUESTION: What Sales command represents "this home is now claimed"?
//   Candidates: SubmitForFunding, ApprovePackage, FinalizePackage, or a dedicated ClaimHome command.
//   Until this is defined, Flow 2 remains unimplemented. The 1:many FK query infrastructure
//   (IInventoryCacheQueries.GetPackageLinesForHomeAsync) is ready for both flows.
internal sealed class RemoveOnLotHomeCacheCommandHandler(
    ICacheWriteScope cacheWriteScope,
    IOnLotHomeCacheWriter cacheWriter,
    IInventoryCacheQueries cacheQueries,
    ILogger<RemoveOnLotHomeCacheCommandHandler> logger)
    : ICommandHandler<RemoveOnLotHomeCacheCommand>
{
    public async Task<Result> Handle(
        RemoveOnLotHomeCacheCommand request,
        CancellationToken cancellationToken)
    {
        using var _ = cacheWriteScope.AllowWrites();

        var affectedLines = await cacheQueries.GetPackageLinesForHomeByRefIdAsync(
            request.RefOnLotHomeId,
            cancellationToken);

        if (affectedLines.Count > 0)
        {
            logger.LogWarning(
                "OnLotHome {StockNumber} (HC={HomeCenterNumber}) removed from inventory — {Count} package line(s) will lose product reference: {Affected}",
                request.StockNumber,
                request.HomeCenterNumber,
                affectedLines.Count,
                string.Join(", ", affectedLines.Select(l => $"Sale={l.SaleId}/Pkg={l.PackageId}/Line={l.PackageLineId}")));

            // TODO: For each affected package, dispatch a command to flag the HomeLine
            // as "product no longer in inventory." The salesperson should see a warning
            // and can either select a different home or convert to manual entry.
        }

        await cacheWriter.RemoveByRefIdAsync(request.RefOnLotHomeId, cancellationToken);

        return Result.Success();
    }
}
