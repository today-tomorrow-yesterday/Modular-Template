using Modules.Inventory.Domain;
using Modules.Inventory.Domain.LandParcels;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.LandInventory.SyncLandParcelFromCdc;

// Flow: CDC feed → SyncLandParcelFromCdcCommand → upsert LandParcel aggregate
internal sealed class SyncLandParcelFromCdcCommandHandler(
    ILandParcelRepository repository,
    IUnitOfWork<IInventoryModule> unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SyncLandParcelFromCdcCommand>
{
    public async Task<Result> Handle(
        SyncLandParcelFromCdcCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(request.LandParcelId, cancellationToken);

        if (existing is not null)
        {
            existing.ReviseAppraisal(
                request.LandCost,
                request.Appraisal,
                dateTimeProvider.UtcNow);

            existing.ReviseDetails(
                request.StockType,
                request.LandAge,
                request.AddToTotal,
                request.MapParcel,
                request.Address,
                request.Address2,
                request.City,
                request.State,
                request.Zip,
                request.County,
                request.LoanNumber,
                request.HomeStockNumber,
                dateTimeProvider.UtcNow);
        }
        else
        {
            var parcel = LandParcel.Create(
                request.LandParcelId,
                request.RefHomeCenterNumber,
                request.RefStockNumber,
                request.StockType,
                request.LandAge,
                request.LandCost,
                request.AddToTotal,
                request.Appraisal,
                request.MapParcel,
                request.Address,
                request.Address2,
                request.City,
                request.State,
                request.Zip,
                request.County,
                request.LoanNumber,
                request.HomeStockNumber,
                dateTimeProvider.UtcNow);

            repository.Add(parcel);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
