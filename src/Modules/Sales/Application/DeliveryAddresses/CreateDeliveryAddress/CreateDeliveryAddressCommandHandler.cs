using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.DeliveryAddresses.CreateDeliveryAddress;

// Flow: POST /sales/{publicSaleId}/delivery-address → CreateDeliveryAddressCommand →
//   load sale → guard duplicate → DeliveryAddress.Create → persist → return PublicId
internal sealed class CreateDeliveryAddressCommandHandler(
    ISaleRepository saleRepository,
    IDeliveryAddressRepository deliveryAddressRepository,
    IUnitOfWork<Domain.ISalesModule> unitOfWork)
    : ICommandHandler<CreateDeliveryAddressCommand, CreateDeliveryAddressResult>
{
    public async Task<Result<CreateDeliveryAddressResult>> Handle(
        CreateDeliveryAddressCommand request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale and guard against duplicate delivery address
        var saleResult = await LoadSaleWithoutDeliveryAddressAsync(request.SalePublicId, cancellationToken);
        if (saleResult.IsFailure)
            return Result.Failure<CreateDeliveryAddressResult>(saleResult.Error);

        // Step 2: Create delivery address from request
        var deliveryAddress = CreateDeliveryAddressFromRequest(saleResult.Value.Id, request);

        // Step 3: Persist
        deliveryAddressRepository.Add(deliveryAddress);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Step 4: Return created delivery address identifier
        return new CreateDeliveryAddressResult(deliveryAddress.PublicId);
    }

    private async Task<Result<Sale>> LoadSaleWithoutDeliveryAddressAsync(
        Guid salePublicId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdWithDeliveryAddressAsync(
            salePublicId, cancellationToken);

        if (sale is null)
            return Result.Failure<Sale>(SaleErrors.NotFoundByPublicId(salePublicId));

        if (sale.DeliveryAddress is not null)
            return Result.Failure<Sale>(DeliveryAddressErrors.AlreadyExists(sale.Id));

        return sale;
    }

    private static DeliveryAddress CreateDeliveryAddressFromRequest(
        int saleId, CreateDeliveryAddressCommand request)
    {
        return DeliveryAddress.Create(
            saleId,
            request.OccupancyType,
            request.IsWithinCityLimits,
            addressStyle: null,
            addressType: null,
            request.AddressLine1,
            addressLine2: null,
            addressLine3: null,
            request.City,
            request.County,
            request.State?.ToUpperInvariant(),
            country: null,
            request.PostalCode);
    }
}
