using Modules.SampleOrders.Domain.Customers;
using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Customers.AddContact;

public sealed record AddContactCommand(
    Guid PublicCustomerId,
    ContactType Type,
    string Value,
    bool IsPrimary) : ICommand;
