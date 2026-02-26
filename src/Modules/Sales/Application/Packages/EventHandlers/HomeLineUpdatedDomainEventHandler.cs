using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;

namespace Modules.Sales.Application.Packages.EventHandlers;

// Flow: HomeLineUpdatedDomainEvent → check occupancy eligibility → recalculate HomeFirst premium if needed
// Fired automatically by Package.AddLine(HomeLine) in the domain.
internal sealed class HomeLineUpdatedDomainEventHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : DomainEventHandler<HomeLineUpdatedDomainEvent>
{
    public override async Task Handle(
        HomeLineUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var package = await packageRepository
            .GetByIdWithSaleContextAsync(domainEvent.PackageId, cancellationToken);

        if (package is null)
        {
            return;
        }

        // Step 1: Check occupancy eligibility — if delivery address occupancy is
        // Rental or Investment, HomeFirst Insurance and Warranty lines are ineligible.
        // Remove only HomeFirst (not Outside Insurance) to match legacy behavior.
        var deliveryAddress = package.Sale.DeliveryAddress;
        if (deliveryAddress is not null &&
            DeliveryAddress.IsOccupancyInsuranceIneligible(deliveryAddress.OccupancyType))
        {
            package.RemoveHomeFirstInsuranceLine();
            package.RemoveLine<WarrantyLine>();
        }

        // Step 2: Remove stale HomeFirst insurance quote — home dimensions changed,
        // premium is no longer valid. User must call the insurance quote endpoint
        // to get a fresh premium with updated home details.
        // (Cannot re-quote here because original user-input parameters like mailing
        // address, birth dates, and coverage amount are not stored on the InsuranceLine.)
        package.RemoveHomeFirstInsuranceLine();

        package.RecalculateGrossProfit();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
