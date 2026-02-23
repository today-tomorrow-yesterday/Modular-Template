using Modules.Customer.Domain;
using Modules.Customer.Infrastructure.Persistence;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;

namespace Modules.Customer.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<ICustomerModule> dbConnectionFactory)
    : IdempotentDomainEventHandlerBase<TDomainEvent, ICustomerModule>(decorated, dbConnectionFactory)
    where TDomainEvent : IDomainEvent
{
    protected override string Schema => Schemas.Customers;
}
