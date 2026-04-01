using Modules.SampleOrders.Application.Orders.GetOrder;
using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.GetOrders;

public sealed record GetOrdersQuery(int? Limit = 100) : IQuery<IReadOnlyCollection<OrderResponse>>;
