using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.Orders.Events;
using Modules.SampleOrders.IntegrationEvents;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

internal sealed class OrderPlacedDomainEventHandler(
    IOrderRepository orderRepository,
    ICustomerRepository customerRepository,
    IEventBus eventBus,
    IDateTimeProvider dateTimeProvider) : DomainEventHandler<OrderPlacedDomainEvent>
{
    public override async Task Handle(
        OrderPlacedDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdAsync(
            domainEvent.EntityId,
            cancellationToken);

        if (order is null) return;

        var customer = await customerRepository.GetByIdAsync(
            order.CustomerId,
            cancellationToken);

        var lines = order.Lines.Select(l => new OrderLineDto(
            Guid.Empty, // ProductId lookup would require cache query — simplified for template
            l.Quantity,
            l.UnitPrice.Amount,
            l.UnitPrice.Currency)).ToList();

        await eventBus.PublishAsync(
            new OrderPlacedIntegrationEvent(
                Guid.CreateVersion7(),
                dateTimeProvider.UtcNow,
                order.PublicId,
                customer?.PublicId ?? Guid.Empty,
                lines,
                order.TotalPrice.Amount,
                order.TotalPrice.Currency,
                order.Status.ToString(),
                order.OrderedAtUtc),
            cancellationToken);
    }
}
