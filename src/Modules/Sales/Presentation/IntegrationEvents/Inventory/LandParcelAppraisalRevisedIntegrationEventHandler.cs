using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.ReviseLandParcelCacheAppraisal;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.LandParcelAppraisalRevised (EventBridge) → Send ReviseLandParcelCacheAppraisalCommand → Upsert cache.land_parcels
internal sealed class LandParcelAppraisalRevisedIntegrationEventHandler(
    ISender sender,
    ILogger<LandParcelAppraisalRevisedIntegrationEventHandler> logger)
    : IntegrationEventHandler<LandParcelAppraisalRevisedIntegrationEvent>
{
    public override async Task HandleAsync(
        LandParcelAppraisalRevisedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing LandParcelAppraisalRevised: LandParcelId={LandParcelId}, Stock={StockNumber}",
            integrationEvent.LandParcelId,
            integrationEvent.StockNumber);

        await sender.Send(
            new ReviseLandParcelCacheAppraisalCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    private static LandParcelCache MapToCache(LandParcelAppraisalRevisedIntegrationEvent e) => new()
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
