using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.CreateOnLotHomeCache;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.OnLotHomeAddedToInventory (EventBridge) → Send CreateOnLotHomeCacheCommand → Insert cache.on_lot_homes
internal sealed class OnLotHomeAddedToInventoryIntegrationEventHandler(
    ISender sender,
    ILogger<OnLotHomeAddedToInventoryIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OnLotHomeAddedToInventoryIntegrationEvent>
{
    public async Task HandleAsync(
        OnLotHomeAddedToInventoryIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OnLotHomeAddedToInventory: OnLotHomeId={OnLotHomeId}, HC={HomeCenterNumber}, Stock={StockNumber}",
            integrationEvent.OnLotHomeId,
            integrationEvent.HomeCenterNumber,
            integrationEvent.StockNumber);

        await sender.Send(
            new CreateOnLotHomeCacheCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((OnLotHomeAddedToInventoryIntegrationEvent)integrationEvent, cancellationToken);

    private static OnLotHomeCache MapToCache(OnLotHomeAddedToInventoryIntegrationEvent e) => new()
    {
        RefOnLotHomeId = e.OnLotHomeId,
        RefHomeCenterNumber = e.HomeCenterNumber,
        RefStockNumber = e.StockNumber,
        StockType = e.StockType,
        Condition = Enum.TryParse<HomeCondition>(e.Condition, out var c) ? c : null,
        BuildType = e.BuildType,
        Width = e.Width,
        Length = e.Length,
        NumberOfBedrooms = e.NumberOfBedrooms,
        NumberOfBathrooms = e.NumberOfBathrooms,
        ModelYear = e.ModelYear,
        Model = e.Model,
        Make = e.Make,
        Facility = e.Facility,
        SerialNumber = e.SerialNumber,
        TotalInvoiceAmount = e.TotalInvoiceAmount,
        OriginalRetailPrice = e.OriginalRetailPrice,
        CurrentRetailPrice = e.CurrentRetailPrice
    };
}
