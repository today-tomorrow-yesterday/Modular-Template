using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCachePrice;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.OnLotHomePriceRevised (EventBridge) → Send ReviseOnLotHomeCachePriceCommand → Upsert cache.on_lot_homes
internal sealed class OnLotHomePriceRevisedIntegrationEventHandler(
    ISender sender,
    ILogger<OnLotHomePriceRevisedIntegrationEventHandler> logger)
    : IIntegrationEventHandler<OnLotHomePriceRevisedIntegrationEvent>
{
    public async Task HandleAsync(
        OnLotHomePriceRevisedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OnLotHomePriceRevised: OnLotHomeId={OnLotHomeId}, Stock={StockNumber}",
            integrationEvent.OnLotHomeId,
            integrationEvent.StockNumber);

        await sender.Send(
            new ReviseOnLotHomeCachePriceCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    public Task HandleAsync(
        IIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
        => HandleAsync((OnLotHomePriceRevisedIntegrationEvent)integrationEvent, cancellationToken);

    private static OnLotHomeCache MapToCache(OnLotHomePriceRevisedIntegrationEvent e) => new()
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
