using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.CreateCustomer;

public sealed record CreateCustomerCommand(
    string FirstName,
    string? MiddleName,
    string LastName,
    string? Email,
    DateOnly? DateOfBirth = null) : ICommand<Guid>;
