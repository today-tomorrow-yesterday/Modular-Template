using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.SetShippingAddress;

public sealed record SetShippingAddressCommand(
    Guid PublicOrderId,
    string AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country) : ICommand;
