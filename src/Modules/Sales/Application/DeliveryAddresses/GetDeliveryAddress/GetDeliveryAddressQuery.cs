using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.DeliveryAddresses.GetDeliveryAddress;

public sealed record GetDeliveryAddressQuery(Guid SalePublicId) : IQuery<GetDeliveryAddressResult>;

public sealed record GetDeliveryAddressResult(
    int Id,
    int SaleId,
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? County,
    string? State,
    string? PostalCode);
