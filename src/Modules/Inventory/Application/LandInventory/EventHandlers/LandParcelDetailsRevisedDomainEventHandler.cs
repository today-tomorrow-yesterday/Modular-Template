using Modules.Inventory.Domain.LandParcels;
using Modules.Inventory.Domain.LandParcels.Events;
using Modules.Inventory.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Inventory.Application.LandInventory.EventHandlers;

// Flow: Inventory.LandParcelDetailsRevisedDomainEvent → publishes Inventory.LandParcelDetailsRevisedIntegrationEvent
internal sealed class LandParcelDetailsRevisedDomainEventHandler(
    ILandParcelRepository repository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<LandParcelDetailsRevisedDomainEvent>
{
    public override async Task Handle(
        LandParcelDetailsRevisedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var parcel = await repository.GetByIdAsync(domainEvent.EntityId, cancellationToken);

        if (parcel is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new LandParcelDetailsRevisedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                parcel.Id,
                parcel.RefHomeCenterNumber,
                parcel.RefStockNumber,
                parcel.StockType,
                parcel.LandCost,
                parcel.Appraisal,
                parcel.Address,
                parcel.City,
                parcel.State,
                parcel.Zip,
                parcel.County),
            cancellationToken);
    }
}
