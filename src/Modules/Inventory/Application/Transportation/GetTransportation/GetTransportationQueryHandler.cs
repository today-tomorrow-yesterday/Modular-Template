using Modules.Inventory.Domain.OnLotHomes;
using Modules.Inventory.Domain.WheelsAndAxles;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.Transportation.GetTransportation;

// Flow: GET /api/v1/inventory/transportation → Inventory.on_lot_homes by dimensions → latest Inventory.wheels_and_axles → derive axle/wheel counts
internal sealed class GetTransportationQueryHandler(
    IOnLotHomeRepository onLotHomeRepository,
    IWheelsAndAxlesTransactionRepository wheelsAndAxlesRepository)
    : IQueryHandler<GetTransportationQuery, TransportationResponse>
{
    public async Task<Result<TransportationResponse>> Handle(
        GetTransportationQuery request,
        CancellationToken cancellationToken)
    {
        // Find homes matching the given dimensions
        var matchingHomes = await onLotHomeRepository.GetByDimensionsAsync(
            request.Length, request.Width, cancellationToken);

        if (matchingHomes.Count == 0)
        {
            return Result.Failure<TransportationResponse>(
                TransportationErrors.NoDimensionData(request.Length, request.Width));
        }

        var stockNumbers = matchingHomes.Select(h => h.RefStockNumber).ToHashSet();

        // Get the most recent W&A transaction for any of these stock numbers
        var transaction = await wheelsAndAxlesRepository.GetLatestByStockNumbersAsync(
            stockNumbers, cancellationToken);

        if (transaction is null)
        {
            return Result.Failure<TransportationResponse>(
                TransportationErrors.NoDimensionData(request.Length, request.Width));
        }

        return new TransportationResponse(
            NumberOfAxles: transaction.BrakeAxles + transaction.IdlerAxles,
            NumberOfWheels: transaction.Wheels);
    }
}
