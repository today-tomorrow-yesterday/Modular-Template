using Modules.SampleSales.Domain;
using Modules.SampleSales.Infrastructure.Persistence;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Inbox.Handlers;

namespace Modules.SampleSales.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ISampleSalesModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ISampleSalesModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Sample;
}
