using Modules.Funding.Domain;
using Modules.Funding.Infrastructure.Persistence;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;

namespace Modules.Funding.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<IFundingModule> dbConnectionFactory)
    : IdempotentDomainEventHandlerBase<TDomainEvent, IFundingModule>(decorated, dbConnectionFactory)
    where TDomainEvent : IDomainEvent
{
    protected override string Schema => Schemas.Fundings;
}
