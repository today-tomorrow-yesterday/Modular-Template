using Modules.SampleOrders.Domain.Orders;
using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    Guid PublicOrderId,
    OrderStatus NewStatus) : ICommand;
