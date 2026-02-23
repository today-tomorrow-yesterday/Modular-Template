using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.CreateLandParcelCache;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.LandParcelAddedToInventory (EventBridge) → Send CreateLandParcelCacheCommand → Insert cache.land_parcels
internal sealed class LandParcelAddedToInventoryIntegrationEventHandler(
    ISender sender,
    ILogger<LandParcelAddedToInventoryIntegrationEventHandler> logger)
    : IIntegrationEventHandler<LandParcelAddedToInventoryIntegrationEvent>
{
    public async Task HandleAsync(
        LandParcelAddedToInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing LandParcelAddedToInventory: LandParcelId={LandParcelId}, HC={HomeCenterNumber}, Stock={StockNumber}",
            integrationEvent.LandParcelId,
            integrationEvent.HomeCenterNumber,
            integrationEvent.StockNumber);

        await sender.Send(
            new CreateLandParcelCacheCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((LandParcelAddedToInventoryIntegrationEvent)integrationEvent, cancellationToken);

    private static LandParcelCache MapToCache(LandParcelAddedToInventoryIntegrationEvent e) => new()
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
