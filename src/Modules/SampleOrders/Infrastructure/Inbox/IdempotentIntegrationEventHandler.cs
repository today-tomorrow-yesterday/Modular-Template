using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Infrastructure.Persistence;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Inbox.Handlers;

namespace Modules.SampleOrders.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ISampleOrdersModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Orders;
}
