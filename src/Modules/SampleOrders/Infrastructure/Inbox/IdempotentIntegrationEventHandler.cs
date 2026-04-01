using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Infrastructure.Persistence;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Inbox.Handlers;

namespace Modules.SampleOrders.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ISampleOrdersModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Orders;
}
