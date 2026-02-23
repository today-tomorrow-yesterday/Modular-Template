using Modules.Inventory.Domain.LandParcels.Events;
using Modules.Inventory.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Inventory.Application.LandInventory.EventHandlers;

// Flow: Inventory.LandParcelRemovedDomainEvent → publishes Inventory.LandParcelRemovedFromInventoryIntegrationEvent
internal sealed class LandParcelRemovedDomainEventHandler(
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<LandParcelRemovedDomainEvent>
{
    public override async Task Handle(
        LandParcelRemovedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new LandParcelRemovedFromInventoryIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                domainEvent.EntityId,
                domainEvent.HomeCenterNumber,
                domainEvent.StockNumber),
            cancellationToken);
    }
}
