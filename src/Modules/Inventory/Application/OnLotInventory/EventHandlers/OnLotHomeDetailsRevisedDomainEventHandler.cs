using Modules.Inventory.Domain.OnLotHomes;
using Modules.Inventory.Domain.OnLotHomes.Events;
using Modules.Inventory.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.Inventory.Application.OnLotInventory.EventHandlers;

// Flow: Inventory.OnLotHomeDetailsRevisedDomainEvent → publishes Inventory.OnLotHomeDetailsRevisedIntegrationEvent
internal sealed class OnLotHomeDetailsRevisedDomainEventHandler(
    IOnLotHomeRepository repository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OnLotHomeDetailsRevisedDomainEvent>
{
    public override async Task Handle(
        OnLotHomeDetailsRevisedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var home = await repository.GetByIdAsync(domainEvent.EntityId, cancellationToken);

        if (home is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new OnLotHomeDetailsRevisedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                home.Id,
                home.RefHomeCenterNumber,
                home.RefStockNumber,
                home.StockType,
                home.Condition,
                home.BuildType,
                home.Width,
                home.Length,
                home.NumberOfBedrooms,
                home.NumberOfBathrooms,
                home.ModelYear,
                home.Model,
                home.Make,
                home.Facility,
                home.SerialNumber,
                home.TotalInvoiceAmount,
                home.OriginalRetailPrice,
                home.CurrentRetailPrice),
            cancellationToken);
    }
}
