using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.DeliveryAddresses.Events;
using Modules.Sales.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.DeliveryAddresses.EventHandlers;

// Flow: Sales.DeliveryAddressCreated → publishes Sales.DeliveryAddressCreatedIntegrationEvent
internal sealed class DeliveryAddressCreatedDomainEventHandler(
    IDeliveryAddressRepository deliveryAddressRepository,
    IEventBus eventBus)
    : DomainEventHandler<DeliveryAddressCreatedDomainEvent>
{
    public override async Task Handle(
        DeliveryAddressCreatedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var deliveryAddress = await deliveryAddressRepository
            .GetByIdAsync(domainEvent.DeliveryAddressId, cancellationToken);

        if (deliveryAddress is null)
        {
            return;
        }

        await eventBus.PublishAsync(
            new DeliveryAddressCreatedIntegrationEvent(
                Guid.CreateVersion7(),
                domainEvent.OccurredOnUtc,
                deliveryAddress.Sale.PublicId,
                deliveryAddress.Sale.Customer?.RefPublicId,
                deliveryAddress.OccupancyType,
                deliveryAddress.IsWithinCityLimits,
                deliveryAddress.AddressLine1,
                deliveryAddress.City,
                deliveryAddress.County,
                deliveryAddress.State,
                deliveryAddress.PostalCode),
            cancellationToken);
    }
}
