using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.IntegrationEvents;
using Modules.Sales.Application.InventoryCache.ReviseOnLotHomeCacheDetails;
using Modules.Sales.Domain.InventoryCache;
using Rtl.Core.Application.EventBus;

namespace Modules.Sales.Presentation.IntegrationEvents.Inventory;

// Flow: Inventory.OnLotHomeDetailsRevised (EventBridge) → Send ReviseOnLotHomeCacheDetailsCommand → Upsert cache.on_lot_homes
internal sealed class OnLotHomeDetailsRevisedIntegrationEventHandler(
    ISender sender,
    ILogger<OnLotHomeDetailsRevisedIntegrationEventHandler> logger)
    : IntegrationEventHandler<OnLotHomeDetailsRevisedIntegrationEvent>
{
    public override async Task HandleAsync(
        OnLotHomeDetailsRevisedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing OnLotHomeDetailsRevised: PublicOnLotHomeId={PublicOnLotHomeId}, Stock={StockNumber}",
            integrationEvent.PublicOnLotHomeId,
            integrationEvent.StockNumber);

        await sender.Send(
            new ReviseOnLotHomeCacheDetailsCommand(MapToCache(integrationEvent)),
            cancellationToken);
    }

    private static OnLotHomeCache MapToCache(OnLotHomeDetailsRevisedIntegrationEvent e) => new()
    {
        RefPublicId = e.PublicOnLotHomeId,
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
