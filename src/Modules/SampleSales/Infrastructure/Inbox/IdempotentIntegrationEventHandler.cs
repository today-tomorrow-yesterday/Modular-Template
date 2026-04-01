using Modules.SampleSales.Domain;
using Modules.SampleSales.Infrastructure.Persistence;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Inbox.Handlers;

namespace Modules.SampleSales.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ISampleSalesModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ISampleSalesModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Sample;
}
