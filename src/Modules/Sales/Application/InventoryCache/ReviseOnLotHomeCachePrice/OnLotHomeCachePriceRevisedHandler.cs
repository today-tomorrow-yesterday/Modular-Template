using Microsoft.Extensions.Logging;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.InventoryCache.Events;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCachePrice;

// Flow: Sales.OnLotHomeCachePriceRevised → flag affected package lines for pricing review
internal sealed class OnLotHomeCachePriceRevisedHandler(
    IInventoryCacheQueries cacheQueries,
    ILogger<OnLotHomeCachePriceRevisedHandler> logger)
    : DomainEventHandler<OnLotHomeCachePriceRevisedDomainEvent>
{
    public override async Task Handle(
        OnLotHomeCachePriceRevisedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var affectedLines = await cacheQueries.GetPackageLinesForHomeAsync(
            domainEvent.OnLotHomeCacheId,
            cancellationToken);

        if (affectedLines.Count == 0)
        {
            return;
        }

        logger.LogInformation(
            "OnLotHome {StockNumber} price revised — {Count} package line(s) may need pricing review: {Affected}",
            domainEvent.StockNumber,
            affectedLines.Count,
            string.Join(", ", affectedLines.Select(l => $"Sale={l.SaleId}/Pkg={l.PackageId}/Line={l.PackageLineId}")));

        // TODO: For each affected package, dispatch a command to flag MustRecalculatePricing.
        // Pricing depends on CurrentRetailPrice (changed) → tax, W&A, and commission all cascade.
        // Use lazy recalculation: set the flag now, recalculate when the salesperson opens the package.
        // The Package aggregate method (e.g. FlagForPricingReview) would raise a domain event
        // that could surface as a UI notification ("prices have changed on this package").
    }
}
