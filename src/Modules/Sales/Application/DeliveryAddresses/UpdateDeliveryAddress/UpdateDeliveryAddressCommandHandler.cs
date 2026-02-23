using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;

// Flow: Sales.UpdateDeliveryAddressCommand → update Sales.delivery_addresses → raises Sales.DeliveryAddressChanged / StateChanged / LocationChanged
internal sealed class UpdateDeliveryAddressCommandHandler(
    ISaleRepository saleRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : ICommandHandler<UpdateDeliveryAddressCommand>
{
    public async Task<Result> Handle(
        UpdateDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale and validate delivery address exists
        var addressResult = await LoadExistingDeliveryAddressAsync(request.SalePublicId, cancellationToken);
        if (addressResult.IsFailure)
            return Result.Failure(addressResult.Error);

        // Step 2: Apply address updates (raises domain events for state/location changes)
        ApplyAddressUpdates(addressResult.Value, request);

        // Step 3: Persist
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<DeliveryAddress>> LoadExistingDeliveryAddressAsync(
        Guid salePublicId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithDeliveryAddressAsync(
            salePublicId, cancellationToken);

        if (sale is null)
            return Result.Failure<DeliveryAddress>(SaleErrors.NotFoundByPublicId(salePublicId));

        if (sale.DeliveryAddress is null)
            return Result.Failure<DeliveryAddress>(DeliveryAddressErrors.NotFound(sale.Id));

        return sale.DeliveryAddress;
    }

    private static void ApplyAddressUpdates(
        DeliveryAddress deliveryAddress, UpdateDeliveryAddressCommand request)
    {
        deliveryAddress.Update(
            request.OccupancyType,
            request.IsWithinCityLimits,
            addressStyle: null,
            addressType: null,
            request.AddressLine1,
            addressLine2: null,
            addressLine3: null,
            request.City,
            request.County,
            NormalizeStateAbbreviation(request.State),
            country: null,
            request.PostalCode);
    }

    // Validator enforces MaxLength(2) — full state names never reach the handler.
    // Just normalize casing for iSeries compatibility.
    private static string? NormalizeStateAbbreviation(string? state) =>
        string.IsNullOrWhiteSpace(state) ? state : state.ToUpperInvariant();
}
