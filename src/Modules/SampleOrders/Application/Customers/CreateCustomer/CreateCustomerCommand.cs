using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string Name,
    string Email) : ICommand<int>;
