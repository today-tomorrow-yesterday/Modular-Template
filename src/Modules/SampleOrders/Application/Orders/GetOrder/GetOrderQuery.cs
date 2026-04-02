using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.GetOrder;

public sealed record GetOrderQuery(Guid PublicOrderId) : IQuery<OrderResponse>;
