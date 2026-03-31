using Modules.Inventory.Domain.LandCosts;
using Modules.Inventory.Domain.OnLotHomes;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.OnLotInventory.GetOnLotInventory;

// Flow: GET /api/v1/inventory/on-lot → join Inventory.on_lot_homes + land_costs + ancillary_data
internal sealed class GetOnLotInventoryQueryHandler(
    IOnLotHomeRepository onLotHomeRepository,
    ILandCostRepository landCostRepository,
    Domain.AncillaryData.IAncillaryDataRepository ancillaryDataRepository)
    : IQueryHandler<GetOnLotInventoryQuery, IReadOnlyCollection<OnLotInventoryResponse>>
{
    public async Task<Result<IReadOnlyCollection<OnLotInventoryResponse>>> Handle(
        GetOnLotInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var homes = await onLotHomeRepository.GetByHomeCenterNumberAsync(
            request.HomeCenterNumber, cancellationToken);

        if (homes.Count == 0)
        {
            return Result.Success<IReadOnlyCollection<OnLotInventoryResponse>>(
                Array.Empty<OnLotInventoryResponse>());
        }

        var stockNumbers = homes.Select(h => h.RefStockNumber).ToHashSet();

        var landCosts = await landCostRepository.GetByHomeCenterAndStockNumbersAsync(
            request.HomeCenterNumber, stockNumbers, cancellationToken);

        var ancillaryData = await ancillaryDataRepository.GetByHomeCenterAndStockNumbersAsync(
            request.HomeCenterNumber, stockNumbers, cancellationToken);

        var landCostsByStock = landCosts.ToDictionary(l => l.RefStockNumber);
        var ancillaryByStock = ancillaryData.ToDictionary(a => a.RefStockNumber);

        var responses = homes.Select(home =>
        {
            landCostsByStock.TryGetValue(home.RefStockNumber, out var lc);
            ancillaryByStock.TryGetValue(home.RefStockNumber, out var ad);

            return new OnLotInventoryResponse(
                home.PublicId,
                home.RefHomeCenterNumber,
                home.RefStockNumber,
                home.StockType,
                home.Condition,
                home.BuildType,
                home.Width,
                home.Length,
                home.NumberOfBedrooms,
                home.NumberOfBathrooms,
                home.ModelYear,
                home.TotalInvoiceAmount,
                home.PurchaseDiscount,
                home.OriginalRetailPrice,
                home.CurrentRetailPrice,
                home.Model,
                home.Make,
                home.Facility,
                home.SerialNumber,
                home.StockedInDate,
                home.LandStockNumber,
                lc is not null ? new OnLotLandCostsResponse(lc.AddToTotal, lc.FurnitureTotal) : null,
                ad is not null ? new OnLotAncillaryDataResponse(ad.CustomerName, ad.PackageReceivedDate) : null);
        }).ToList();

        return Result.Success<IReadOnlyCollection<OnLotInventoryResponse>>(responses);
    }
}
