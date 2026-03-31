using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.SetShippingAddress;

public sealed record SetShippingAddressCommand(
    int OrderId,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country) : ICommand;
