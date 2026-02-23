using Modules.Inventory.Domain;
using Modules.Inventory.Domain.LandParcels;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.LandInventory.RemoveLandParcelFromCdc;

// Flow: CDC feed delete → RemoveLandParcelFromCdcCommand → MarkRemoved + Remove
internal sealed class RemoveLandParcelFromCdcCommandHandler(
    ILandParcelRepository repository,
    IUnitOfWork<IInventoryModule> unitOfWork) : ICommandHandler<RemoveLandParcelFromCdcCommand>
{
    public async Task<Result> Handle(
        RemoveLandParcelFromCdcCommand request,
        CancellationToken cancellationToken)
    {
        var parcel = await repository.GetByIdAsync(request.LandParcelId, cancellationToken);

        if (parcel is null)
        {
            return Result.Success();
        }

        parcel.MarkRemoved();
        repository.Remove(parcel);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
