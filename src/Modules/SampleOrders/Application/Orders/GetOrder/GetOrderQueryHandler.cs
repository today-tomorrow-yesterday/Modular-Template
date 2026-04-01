using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Domain.Results;

namespace Modules.SampleOrders.Application.Orders.GetOrder;

internal sealed class GetOrderQueryHandler(IOrderRepository orderRepository)
    : IQueryHandler<GetOrderQuery, OrderResponse>
{
    public async Task<Result<OrderResponse>> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        Order? order = await orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
        {
            return Result.Failure<OrderResponse>(OrderErrors.NotFound(request.OrderId));
        }

        var lines = order.Lines.Select(l => new OrderLineResponse(
            l.GetType().Name,
            l.Quantity,
            l.UnitPrice.Amount,
            l.UnitPrice.Currency,
            l.LineTotal.Amount)).ToList();

        var shippingAddress = order.ShippingAddress is not null
            ? new ShippingAddressResponse(
                order.ShippingAddress.Address.AddressLine1,
                order.ShippingAddress.Address.AddressLine2,
                order.ShippingAddress.Address.City,
                order.ShippingAddress.Address.State,
                order.ShippingAddress.Address.PostalCode,
                order.ShippingAddress.Address.Country)
            : null;

        return new OrderResponse(
            order.PublicId,
            lines,
            order.TotalPrice.Amount,
            order.TotalPrice.Currency,
            order.Status,
            order.OrderedAtUtc,
            shippingAddress,
            order.CreatedAtUtc,
            order.CreatedByUserId,
            order.ModifiedAtUtc,
            order.ModifiedByUserId);
    }
}
