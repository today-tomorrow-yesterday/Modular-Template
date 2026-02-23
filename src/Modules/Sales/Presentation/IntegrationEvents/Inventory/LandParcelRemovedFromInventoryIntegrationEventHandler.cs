using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.RemoveLandParcelCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.LandParcelRemovedFromInventory (EventBridge) → Send RemoveLandParcelCacheCommand → Delete cache.land_parcels
internal sealed class LandParcelRemovedFromInventoryIntegrationEventHandler(
    ISender sender,
    ILogger<LandParcelRemovedFromInventoryIntegrationEventHandler> logger)
    : IIntegrationEventHandler<LandParcelRemovedFromInventoryIntegrationEvent>
{
    public async Task HandleAsync(
        LandParcelRemovedFromInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Processing LandParcelRemovedFromInventory: LandParcelId={LandParcelId}, HC={HomeCenterNumber}, Stock={StockNumber}",
            integrationEvent.LandParcelId,
            integrationEvent.HomeCenterNumber,
            integrationEvent.StockNumber);

        await sender.Send(
            new RemoveLandParcelCacheCommand(
                integrationEvent.LandParcelId,
                integrationEvent.HomeCenterNumber,
                integrationEvent.StockNumber),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((LandParcelRemovedFromInventoryIntegrationEvent)integrationEvent, cancellationToken);
}
