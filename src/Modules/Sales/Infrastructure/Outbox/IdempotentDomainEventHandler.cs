using Modules.Sales.Domain;
using Modules.Sales.Infrastructure.Persistence;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;

namespace Modules.Sales.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<ISalesModule> dbConnectionFactory)
    : IdempotentDomainEventHandlerBase<TDomainEvent, ISalesModule>(decorated, dbConnectionFactory)
    where TDomainEvent : IDomainEvent
{
    protected override string Schema => Schemas.Sales;
}
