using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.RemoveOnLotHomeCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.OnLotHomeRemovedFromInventory (EventBridge) → Send RemoveOnLotHomeCacheCommand → Delete cache.on_lot_homes
internal sealed class OnLotHomeRemovedFromInventoryIntegrationEventHandler(
    ISender sender,
    ILogger<OnLotHomeRemovedFromInventoryIntegrationEventHandler> logger)
    : IntegrationEventHandler<OnLotHomeRemovedFromInventoryIntegrationEvent>
{
    public override async Task HandleAsync(
        OnLotHomeRemovedFromInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Processing OnLotHomeRemovedFromInventory: OnLotHomeId={OnLotHomeId}, HC={HomeCenterNumber}, Stock={StockNumber}",
            integrationEvent.OnLotHomeId,
            integrationEvent.HomeCenterNumber,
            integrationEvent.StockNumber);

        await sender.Send(
            new RemoveOnLotHomeCacheCommand(
                integrationEvent.OnLotHomeId,
                integrationEvent.HomeCenterNumber,
                integrationEvent.StockNumber),
            cancellationToken);
    }
}
