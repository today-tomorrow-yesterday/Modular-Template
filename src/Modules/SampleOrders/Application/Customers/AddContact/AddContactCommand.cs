using Modules.SampleOrders.Domain.Customers;
using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.AddContact;

public sealed record AddContactCommand(
    int CustomerId,
    ContactType Type,
    string Value,
    bool IsPrimary) : ICommand;
