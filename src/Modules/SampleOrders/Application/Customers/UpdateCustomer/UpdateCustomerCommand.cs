using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    int CustomerId,
    string Name,
    string Email) : ICommand;
