using Modules.Sales.Domain.DeliveryAddresses.Events;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;

namespace Modules.Sales.Application.DeliveryAddresses.EventHandlers;

// Flow: Sales.DeliveryAddressStateChanged → clear tax Q&A on draft packages
internal sealed class DeliveryAddressStateChangedDomainEventHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : DomainEventHandler<DeliveryAddressStateChangedDomainEvent>
{
    public override async Task Handle(
        DeliveryAddressStateChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var packages = await packageRepository
            .GetBySaleIdAsync(domainEvent.SaleId, cancellationToken);

        foreach (var package in packages)
        {
            var taxLine = package.Lines
                .OfType<TaxLine>()
                .FirstOrDefault();

            taxLine?.ClearQuestionAnswers();

            package.FlagForTaxRecalculation();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
