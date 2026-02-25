using Modules.Sales.Domain.DeliveryAddresses.Events;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;

namespace Modules.Sales.Application.DeliveryAddresses.EventHandlers;

// Flow: Sales.DeliveryAddressLocationChanged → clear tax calculations on draft packages
internal sealed class DeliveryAddressLocationChangedDomainEventHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : DomainEventHandler<DeliveryAddressLocationChangedDomainEvent>
{
    // Use Tax is a project cost with Category 9, Item 21 in iSeries.
    // Identified by well-known constant; resolved via ItemId → cdc.project_cost_items natural key.
    internal const int UseTaxCategoryNumber = 9;
    internal const int UseTaxItemNumber = 21;

    public override async Task Handle(
        DeliveryAddressLocationChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var packages = await packageRepository
            .GetBySaleIdAsync(domainEvent.SaleId, cancellationToken);

        foreach (var package in packages)
        {
            if (package.Status != PackageStatus.Draft)
            {
                continue;
            }

            // Clear tax calculations on the Tax line
            var taxLine = package.Lines
                .OfType<TaxLine>()
                .FirstOrDefault();

            taxLine?.ClearCalculations();

            // Remove Use Tax ProjectCost (Category 9, Item 21)
            package.RemoveProjectCost(UseTaxCategoryNumber, UseTaxItemNumber);

            package.FlagForTaxRecalculation();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
