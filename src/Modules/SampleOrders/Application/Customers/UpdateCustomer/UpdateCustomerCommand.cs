using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid PublicCustomerId,
    string FirstName,
    string? MiddleName,
    string LastName) : ICommand;
