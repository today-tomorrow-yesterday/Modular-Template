using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.Orders.Events;
using Modules.SampleOrders.IntegrationEvents;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

internal sealed class OrderStatusChangedDomainEventHandler(
    IOrderRepository orderRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OrderStatusChangedDomainEvent>
{
    public override async Task Handle(
        OrderStatusChangedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (order is null) return;

        await eventBus.PublishAsync(
            new OrderStatusChangedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                order.PublicId,
                domainEvent.NewStatus.ToString()),
            cancellationToken);
    }
}
