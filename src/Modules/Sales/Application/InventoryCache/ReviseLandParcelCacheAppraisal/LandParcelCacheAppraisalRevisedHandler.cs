using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.InventoryCache.Events;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheAppraisal;

// Flow: Sales.LandParcelCacheAppraisalRevised → flag affected package lines for pricing review
internal sealed class LandParcelCacheAppraisalRevisedHandler(
    IInventoryCacheQueries cacheQueries,
    ILogger<LandParcelCacheAppraisalRevisedHandler> logger)
    : DomainEventHandler<LandParcelCacheAppraisalRevisedDomainEvent>
{
    public override async Task Handle(
        LandParcelCacheAppraisalRevisedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var affectedLines = await cacheQueries.GetPackageLinesForLandAsync(
            domainEvent.LandParcelCacheId,
            cancellationToken);

        if (affectedLines.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "LandParcel {StockNumber} appraisal revised — {Count} package line(s) may need pricing review: {Affected}",
            domainEvent.StockNumber,
            affectedLines.Count,
            string.Join(", ", affectedLines.Select(l => $"Sale={l.SaleId}/Pkg={l.PackageId}/Line={l.PackageLineId}")));

        // TODO: For each affected package, dispatch a command to flag MustRecalculatePricing.
        // Appraisal affects land cost → tax and gross profit calculations cascade.
        // Use lazy recalculation: set the flag now, recalculate when the salesperson opens the package.
        // The Package aggregate method (e.g. FlagForPricingReview) would raise a domain event
        // that could surface as a UI notification ("land appraisal changed on this package").
    }
}
