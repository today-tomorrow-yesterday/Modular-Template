using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.DeliveryAddresses.CreateDeliveryAddress;

public sealed record CreateDeliveryAddressCommand(
    Guid SalePublicId,
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode) : ICommand<CreateDeliveryAddressResult>;

public sealed record CreateDeliveryAddressResult(Guid PublicId);
