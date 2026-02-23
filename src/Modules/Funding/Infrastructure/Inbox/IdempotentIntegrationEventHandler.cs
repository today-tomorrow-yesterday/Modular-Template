using Modules.Funding.Domain;
using Modules.Funding.Infrastructure.Persistence;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Inbox.Handlers;

namespace Modules.Funding.Infrastructure.Inbox;

internal sealed class IdempotentIntegrationEventHandler<TIntegrationEvent>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<IFundingModule> dbConnectionFactory)
    : IdempotentIntegrationEventHandlerBase<TIntegrationEvent, IFundingModule>(decorated, dbConnectionFactory)
    where TIntegrationEvent : IIntegrationEvent
{
    protected override string Schema => Schemas.Fundings;
}
