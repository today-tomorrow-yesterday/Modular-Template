using Modules.Sales.Domain;
using Modules.Sales.Infrastructure.Persistence;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Inbox.Handlers;

namespace Modules.Sales.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ISalesModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ISalesModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Sales;
}
