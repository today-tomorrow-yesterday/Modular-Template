using Modules.Inventory.Domain.OnLotHomes.Events;
using Modules.Inventory.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Inventory.Application.OnLotInventory.EventHandlers;

// Flow: Inventory.OnLotHomeRemovedDomainEvent → publishes Inventory.OnLotHomeRemovedFromInventoryIntegrationEvent
internal sealed class OnLotHomeRemovedDomainEventHandler(
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OnLotHomeRemovedDomainEvent>
{
    public override async Task Handle(
        OnLotHomeRemovedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new OnLotHomeRemovedFromInventoryIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                domainEvent.PublicId),
            cancellationToken);
    }
}
