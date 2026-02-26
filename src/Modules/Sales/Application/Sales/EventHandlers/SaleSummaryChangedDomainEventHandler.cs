using Modules.Sales.Domain.Packages.Home;
using Modules.Sales.Domain.Sales;
using Modules.Sales.Domain.Sales.Events;
using Modules.Sales.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Sales.EventHandlers;

// Flow: SaleSummaryChangedDomainEvent → load sale with context → publish SaleSummaryChangedIntegrationEvent to Inventory
internal sealed class SaleSummaryChangedDomainEventHandler(
    ISaleRepository saleRepository,
    IEventBus eventBus)
    : DomainEventHandler<SaleSummaryChangedDomainEvent>
{
    public override async Task Handle(
        SaleSummaryChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with party, retail location, and package context
        var sale = await saleRepository.GetByIdWithContextAsync(
            domainEvent.SaleId, cancellationToken);

        if (sale is null)
            return;

        // Step 2: Build integration event from sale context
        var integrationEvent = BuildIntegrationEvent(sale, domainEvent);

        // Step 3: Publish to EventBridge for Inventory consumption
        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }

    private static SaleSummaryChangedIntegrationEvent BuildIntegrationEvent(
        Sale sale, SaleSummaryChangedDomainEvent domainEvent)
    {
        var primaryPackage = sale.Packages.FirstOrDefault(p => p.IsPrimaryPackage);
        var homeLine = primaryPackage?.Lines.OfType<HomeLine>().SingleOrDefault();

        return new SaleSummaryChangedIntegrationEvent(
            Guid.CreateVersion7(),
            domainEvent.OccurredOnUtc,
            StockNumber: homeLine?.Details?.StockNumber,
            SaleId: sale.Id,
            CustomerName: sale.Party?.DisplayName,
            ReceivedInDate: null, // Populated later when delivery is scheduled
            OriginalRetailPrice: homeLine?.RetailSalePrice,
            CurrentRetailPrice: homeLine?.SalePrice,
            UpdatedAt: DateTime.UtcNow);
    }
}
