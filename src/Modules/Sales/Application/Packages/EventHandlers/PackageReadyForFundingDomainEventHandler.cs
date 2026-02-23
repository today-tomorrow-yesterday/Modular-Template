using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Lines;
using Modules.Sales.Domain.Sales;
using Modules.Sales.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.EventHandlers;

// Flow: PackageReadyForFundingDomainEvent → enrich with sale context → publish PackageReadyForFundingIntegrationEvent to Funding
internal sealed class PackageReadyForFundingDomainEventHandler(
    ISaleRepository saleRepository,
    IEventBus eventBus)
    : DomainEventHandler<PackageReadyForFundingDomainEvent>
{
    public override async Task Handle(
        PackageReadyForFundingDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdWithContextAsync(
            domainEvent.SaleId, cancellationToken);

        if (sale is null)
            return;

        var package = sale.Packages.FirstOrDefault(p => p.PublicId == domainEvent.PackagePublicId);
        if (package is null)
            return;

        var homeLine = package.Lines.OfType<HomeLine>().SingleOrDefault();

        var integrationEvent = new PackageReadyForFundingIntegrationEvent(
            Guid.CreateVersion7(),
            domainEvent.OccurredOnUtc,
            SaleId: sale.Id,
            PackageId: package.Id,
            CustomerId: sale.PartyId,
            RequestTypeId: 0, // Derived by Funding based on deal type (cash vs loan)
            RequestAmount: domainEvent.RequestAmount,
            HomeCenterNumber: sale.RetailLocation?.RefHomeCenterNumber,
            LenderId: 0, // Unknown at submission time — populated by Funding after lender assignment
            LenderName: null,
            StockNumber: homeLine?.Details?.StockNumber,
            FundingKeys: []);

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
