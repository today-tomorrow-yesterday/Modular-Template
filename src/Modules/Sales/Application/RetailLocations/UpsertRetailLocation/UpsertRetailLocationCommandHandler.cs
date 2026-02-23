using Modules.Sales.Domain;
using Modules.Sales.Domain.RetailLocations;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.RetailLocations.UpsertRetailLocation;

// Flow: Organization.HomeCenterChanged → Sales.UpsertRetailLocationCommand → upsert sales.retail_locations
// Single-write to RetailLocation — cache.home_centers is REMOVED. retail_locations is the sole Organization data target.
internal sealed class UpsertRetailLocationCommandHandler(
    IRetailLocationRepository retailLocationRepository,
    IUnitOfWork<ISalesModule> unitOfWork)
    : ICommandHandler<UpsertRetailLocationCommand>
{
    public async Task<Result> Handle(
        UpsertRetailLocationCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await retailLocationRepository.GetByHomeCenterNumberAsync(
            request.HomeCenterNumber, cancellationToken);

        if (existing is null)
        {
            var retailLocation = RetailLocation.CreateHomeCenter(
                request.HomeCenterNumber,
                request.Name,
                request.StateCode,
                request.Zip,
                request.IsActive);

            retailLocationRepository.Add(retailLocation);
        }
        else
        {
            existing.UpdateFromHomeCenterChanged(
                request.Name,
                request.StateCode,
                request.Zip,
                request.IsActive,
                organizationMetadata: null);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
