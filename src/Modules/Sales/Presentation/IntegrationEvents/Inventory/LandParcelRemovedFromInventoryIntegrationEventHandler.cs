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
    : IntegrationEventHandler<LandParcelRemovedFromInventoryIntegrationEvent>
{
    public override async Task HandleAsync(
        LandParcelRemovedFromInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Processing LandParcelRemovedFromInventory: PublicLandParcelId={PublicLandParcelId}",
            integrationEvent.PublicLandParcelId);

        await sender.Send(
            new RemoveLandParcelCacheCommand(integrationEvent.PublicLandParcelId),
            cancellationToken);
    }
}
