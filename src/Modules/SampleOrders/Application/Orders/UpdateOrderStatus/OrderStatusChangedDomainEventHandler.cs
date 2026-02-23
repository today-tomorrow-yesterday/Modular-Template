using Modules.SampleOrders.Domain.Orders.Events;
using Modules.SampleOrders.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

internal sealed class OrderStatusChangedDomainEventHandler(
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OrderStatusChangedDomainEvent>
{
    public override async Task Handle(
        OrderStatusChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await eventBus.PublishAsync(
            new OrderStatusChangedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                domainEvent.EntityId,
                domainEvent.NewStatus.ToString()),
            cancellationToken);
    }
}
