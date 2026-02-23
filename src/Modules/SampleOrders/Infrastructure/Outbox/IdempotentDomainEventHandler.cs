using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Infrastructure.Persistence;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;

namespace Modules.SampleOrders.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory)
    : IdempotentDomainEventHandlerBase<TDomainEvent, ISampleOrdersModule>(decorated, dbConnectionFactory)
    where TDomainEvent : IDomainEvent
{
    protected override string Schema => Schemas.Orders;
}
