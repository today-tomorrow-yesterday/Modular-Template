using Modules.Inventory.Domain;
using Modules.Inventory.Infrastructure.Persistence;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;

namespace Modules.Inventory.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<IInventoryModule> dbConnectionFactory)
    : IdempotentDomainEventHandlerBase<TDomainEvent, IInventoryModule>(decorated, dbConnectionFactory)
    where TDomainEvent : IDomainEvent
{
    protected override string Schema => Schemas.Inventories;
}
