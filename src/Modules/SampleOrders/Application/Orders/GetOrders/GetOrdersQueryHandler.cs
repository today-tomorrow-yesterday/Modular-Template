using Modules.SampleOrders.Application.Orders.GetOrder;
using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Orders.GetOrders;

internal sealed class GetOrdersQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrdersQuery, IReadOnlyCollection<OrderResponse>>
{
    public async Task<Result<IReadOnlyCollection<OrderResponse>>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Order> orders = await orderRepository.GetAllAsync(
            request.Limit,
            cancellationToken);

        var response = orders.Select(o => new OrderResponse(
            o.PublicId,
            o.Lines.Select(l => new OrderLineResponse(
                l.GetType().Name,
                l.Quantity,
                l.UnitPrice.Amount,
                l.UnitPrice.Currency,
                l.LineTotal.Amount)).ToList(),
            o.TotalPrice.Amount,
            o.TotalPrice.Currency,
            o.Status,
            o.OrderedAtUtc,
            o.ShippingAddress is not null
                ? new ShippingAddressResponse(
                    o.ShippingAddress.Address.AddressLine1,
                    o.ShippingAddress.Address.AddressLine2,
                    o.ShippingAddress.Address.City,
                    o.ShippingAddress.Address.State,
                    o.ShippingAddress.Address.PostalCode,
                    o.ShippingAddress.Address.Country)
                : null,
            o.CreatedAtUtc,
            o.CreatedByUserId,
            o.ModifiedAtUtc,
            o.ModifiedByUserId)).ToList();

        return response;
    }
}
