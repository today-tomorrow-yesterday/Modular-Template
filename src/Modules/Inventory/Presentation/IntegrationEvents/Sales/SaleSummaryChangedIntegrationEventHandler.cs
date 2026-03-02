using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Inventory.Application.SaleSummariesCache.UpsertSaleSummaryCache;
using Modules.Inventory.Domain.SaleSummariesCache;
using Modules.Sales.IntegrationEvents;
using Rtl.Core.Application.EventBus;

namespace Modules.Inventory.Presentation.IntegrationEvents.Sales;

// Flow: Sales.SaleSummaryChangedIntegrationEvent → Inventory.UpsertSaleSummaryCacheCommand
internal sealed class SaleSummaryChangedIntegrationEventHandler(
    ISender sender,
    ILogger<SaleSummaryChangedIntegrationEventHandler> logger)
    : IntegrationEventHandler<SaleSummaryChangedIntegrationEvent>
{
    public override async Task HandleAsync(
        SaleSummaryChangedIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Processing SaleSummaryChanged: StockNumber={StockNumber}",
            integrationEvent.StockNumber);

        var cache = new SaleSummaryCache
        {
            RefStockNumber = integrationEvent.StockNumber ?? string.Empty,
            SaleId = integrationEvent.SaleId,
            CustomerName = integrationEvent.CustomerName,
            ReceivedInDate = integrationEvent.ReceivedInDate,
            OriginalRetailPrice = integrationEvent.OriginalRetailPrice,
            CurrentRetailPrice = integrationEvent.CurrentRetailPrice
        };

        await sender.Send(new UpsertSaleSummaryCacheCommand(cache), cancellationToken);
    }
}
