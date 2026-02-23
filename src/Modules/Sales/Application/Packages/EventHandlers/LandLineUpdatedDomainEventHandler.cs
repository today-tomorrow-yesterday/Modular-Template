using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Packages.Events;
using Modules.Sales.Domain.Packages.Lines;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;

namespace Modules.Sales.Application.Packages.EventHandlers;

// Flow: LandLineUpdatedDomainEvent → clear TaxDetails.Errors on Tax line
// Fired automatically by Package.AddLine(LandLine) in the domain.
internal sealed class LandLineUpdatedDomainEventHandler(
    IPackageRepository packageRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : DomainEventHandler<LandLineUpdatedDomainEvent>
{
    public override async Task Handle(
        LandLineUpdatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var package = await packageRepository
            .GetByIdWithSaleContextAsync(domainEvent.PackageId, cancellationToken);

        if (package is null)
            return;

        // Clear tax calculation errors — land changes may resolve previous tax failures
        var taxLine = package.Lines
            .OfType<TaxLine>()
            .FirstOrDefault();

        taxLine?.ClearErrors();

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
