using Modules.Sales.Domain.DeliveryAddresses.Events;
using Modules.Sales.Domain.Packages;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;

namespace Modules.Sales.Application.DeliveryAddresses.EventHandlers;

// Flow: Sales.DeliveryAddressOccupancyBecameIneligible → remove insurance/warranty lines from draft packages
internal sealed class DeliveryAddressOccupancyBecameIneligibleDomainEventHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : DomainEventHandler<DeliveryAddressOccupancyBecameIneligibleDomainEvent>
{
    public override async Task Handle(
        DeliveryAddressOccupancyBecameIneligibleDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var packages = await packageRepository
            .GetBySaleIdAsync(domainEvent.SaleId, cancellationToken);

        foreach (var package in packages)
        {
            // Remove only HomeFirst insurance (not Outside Insurance) and Warranty lines.
            // Legacy cascades to ALL packages (not just Draft) — occupancy ineligibility
            // overrides regardless of package status.
            // Legacy only removed HomeFirst specifically — Outside Insurance is unaffected
            // by occupancy type changes.
            package.RemoveHomeFirstInsuranceLine();
            package.RemoveWarrantyLine();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
