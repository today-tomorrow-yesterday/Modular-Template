using Modules.SampleOrders.Application.Orders.GetOrder;
using Modules.SampleOrders.Domain.Orders;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

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
            o.CustomerId,
            o.Lines.Select(l => new OrderLineResponse(
                l.Id,
                l.GetType().Name,
                l.Quantity,
                l.UnitPrice.Amount,
                l.UnitPrice.Currency,
                l.LineTotal.Amount)).ToList(),
            o.TotalPrice.Amount,
            o.TotalPrice.Currency,
            o.Status,
            o.OrderedAtUtc,
            o.CreatedAtUtc,
            o.CreatedByUserId,
            o.ModifiedAtUtc,
            o.ModifiedByUserId)).ToList();

        return response;
    }
}
