using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.DeliveryAddresses.UpdateDeliveryAddress;

public sealed record UpdateDeliveryAddressCommand(
    Guid SalePublicId,
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode) : ICommand;
