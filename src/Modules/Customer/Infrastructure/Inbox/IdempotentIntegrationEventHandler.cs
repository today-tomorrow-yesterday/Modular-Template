using Modules.Customer.Domain;
using Modules.Customer.Infrastructure.Persistence;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Inbox.Handlers;

namespace Modules.Customer.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<ICustomerModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, ICustomerModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Customers;
}
