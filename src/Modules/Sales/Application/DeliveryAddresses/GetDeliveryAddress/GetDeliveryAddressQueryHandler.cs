using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.DeliveryAddresses.GetDeliveryAddress;

internal sealed class GetDeliveryAddressQueryHandler(
    ISaleRepository saleRepository)
    : IQueryHandler<GetDeliveryAddressQuery, GetDeliveryAddressResult>
{
    public async Task<Result<GetDeliveryAddressResult>> Handle(
        GetDeliveryAddressQuery request,
        CancellationToken cancellationToken)
    {
        // Step 1: Load sale with delivery address
        var sale = await saleRepository.GetByPublicIdWithDeliveryAddressAsync(
            request.SalePublicId, cancellationToken);

        if (sale is null)
            return Result.Failure<GetDeliveryAddressResult>(SaleErrors.NotFoundByPublicId(request.SalePublicId));

        if (sale.DeliveryAddress is null)
            return Result.Failure<GetDeliveryAddressResult>(DeliveryAddressErrors.NotFound(sale.Id));

        // Step 2: Map delivery address to response
        var address = sale.DeliveryAddress;
        return new GetDeliveryAddressResult(
            address.Id,
            address.SaleId,
            address.OccupancyType,
            address.IsWithinCityLimits,
            address.AddressLine1,
            address.City,
            address.County,
            address.State,
            address.PostalCode);
    }
}
