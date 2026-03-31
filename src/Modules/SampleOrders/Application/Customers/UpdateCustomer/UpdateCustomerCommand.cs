using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    int CustomerId,
    string FirstName,
    string? MiddleName,
    string LastName) : ICommand;
