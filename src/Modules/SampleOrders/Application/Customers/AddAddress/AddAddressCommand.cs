using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.AddAddress;

public sealed record AddAddressCommand(
    Guid PublicCustomerId,
    string AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country,
    bool IsPrimary) : ICommand;
