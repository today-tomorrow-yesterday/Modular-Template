using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.Orders.Events;
using Modules.SampleOrders.IntegrationEvents;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

internal sealed class OrderPlacedDomainEventHandler(
    IOrderRepository orderRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OrderPlacedDomainEvent>
{
    public override async Task Handle(
        OrderPlacedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        Order? order = await orderRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (order is null)
        {
            return;
        }

        var lines = order.Lines.Select(l => new OrderLineDto(
            l.ProductId,
            l.Quantity,
            l.UnitPrice.Amount,
            l.UnitPrice.Currency)).ToList();

        await eventBus.PublishAsync(
            new OrderPlacedIntegrationEvent(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow,
                order.Id,
                order.CustomerId,
                lines,
                order.TotalPrice.Amount,
                order.TotalPrice.Currency,
                order.Status.ToString(),
                order.OrderedAtUtc),
            cancellationToken);
    }
}
