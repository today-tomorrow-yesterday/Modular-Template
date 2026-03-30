using Modules.Inventory.Domain.LandParcels;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.LandInventory.GetLandInventory;

// Flow: GET /api/v1/inventory/land → query Inventory.land_parcels (stock type filter)
internal sealed class GetLandInventoryQueryHandler(
    ILandParcelRepository landParcelRepository)
    : IQueryHandler<GetLandInventoryQuery, IReadOnlyCollection<LandInventoryResponse>>
{
    private static readonly HashSet<string> AllowedStockTypes = ["CR2SP", "OL2SP", "MIR", "SPWC"];

    public async Task<Result<IReadOnlyCollection<LandInventoryResponse>>> Handle(
        GetLandInventoryQuery request,
        CancellationToken cancellationToken)
    {
        var parcels = await landParcelRepository.GetByHomeCenterNumberAsync(
            request.HomeCenterNumber, cancellationToken);

        var filtered = parcels
            .Where(p => p.StockType is not null && AllowedStockTypes.Contains(p.StockType))
            .Select(p => new LandInventoryResponse(
                p.PublicId,
                p.RefHomeCenterNumber,
                p.RefStockNumber,
                p.StockType,
                p.LandAge,
                p.LandCost,
                p.AddToTotal,
                p.Appraisal,
                p.MapParcel,
                p.Address,
                p.Address2,
                p.City,
                p.State,
                p.Zip,
                p.County,
                p.LoanNumber,
                p.HomeStockNumber))
            .ToList();

        return Result.Success<IReadOnlyCollection<LandInventoryResponse>>(filtered);
    }
}
