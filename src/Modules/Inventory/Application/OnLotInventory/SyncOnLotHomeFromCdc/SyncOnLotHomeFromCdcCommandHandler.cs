using Modules.Inventory.Domain;
using Modules.Inventory.Domain.OnLotHomes;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.OnLotInventory.SyncOnLotHomeFromCdc;

// Flow: CDC feed → SyncOnLotHomeFromCdcCommand → upsert OnLotHome aggregate
internal sealed class SyncOnLotHomeFromCdcCommandHandler(
    IOnLotHomeRepository repository,
    IUnitOfWork<IInventoryModule> unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<SyncOnLotHomeFromCdcCommand>
{
    public async Task<Result> Handle(
        SyncOnLotHomeFromCdcCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await repository.GetByIdAsync(request.OnLotHomeId, cancellationToken);

        if (existing is not null)
        {
            existing.RevisePrice(
                request.TotalInvoiceAmount,
                request.PurchaseDiscount,
                request.OriginalRetailPrice,
                request.CurrentRetailPrice,
                dateTimeProvider.UtcNow);

            existing.ReviseDetails(
                request.StockType,
                request.Condition,
                request.BuildType,
                request.Width,
                request.Length,
                request.NumberOfBedrooms,
                request.NumberOfBathrooms,
                request.ModelYear,
                request.Model,
                request.Make,
                request.Facility,
                request.SerialNumber,
                request.StockedInDate,
                request.LandStockNumber,
                dateTimeProvider.UtcNow);
        }
        else
        {
            var home = OnLotHome.Create(
                request.OnLotHomeId,
                request.RefHomeCenterNumber,
                request.RefStockNumber,
                request.StockType,
                request.Condition,
                request.BuildType,
                request.Width,
                request.Length,
                request.NumberOfBedrooms,
                request.NumberOfBathrooms,
                request.ModelYear,
                request.Model,
                request.Make,
                request.Facility,
                request.SerialNumber,
                request.TotalInvoiceAmount,
                request.PurchaseDiscount,
                request.OriginalRetailPrice,
                request.CurrentRetailPrice,
                request.StockedInDate,
                request.LandStockNumber,
                dateTimeProvider.UtcNow);

            repository.Add(home);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
