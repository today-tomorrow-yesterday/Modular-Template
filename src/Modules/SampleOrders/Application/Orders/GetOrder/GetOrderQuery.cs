using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(int OrderId) : IQuery<OrderResponse>;
