using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheDetails;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.LandParcelDetailsRevised (EventBridge) → Send ReviseLandParcelCacheDetailsCommand → Upsert cache.land_parcels
internal sealed class LandParcelDetailsRevisedIntegrationEventHandler(
    ISender sender,
    ILogger<LandParcelDetailsRevisedIntegrationEventHandler> logger)
    : IntegrationEventHandler<LandParcelDetailsRevisedIntegrationEvent>
{
    public override async Task HandleAsync(
        LandParcelDetailsRevisedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing LandParcelDetailsRevised: LandParcelId={LandParcelId}, Stock={StockNumber}",
            integrationEvent.LandParcelId,
            integrationEvent.StockNumber);

        await sender.Send(
            new ReviseLandParcelCacheDetailsCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    private static LandParcelCache MapToCache(LandParcelDetailsRevisedIntegrationEvent e) => new()
    {
        RefLandParcelId = e.LandParcelId,
        RefHomeCenterNumber = e.HomeCenterNumber,
        RefStockNumber = e.StockNumber,
        StockType = e.StockType,
        LandCost = e.LandCost,
        Appraisal = e.Appraisal,
        Address = e.Address,
        City = e.City,
        State = e.State,
        Zip = e.Zip,
        County = e.County
    };
}
