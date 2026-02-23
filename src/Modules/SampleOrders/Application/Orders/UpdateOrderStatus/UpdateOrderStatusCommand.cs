using Modules.SampleOrders.Domain.Orders;
using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand(
    int OrderId,
    OrderStatus NewStatus) : ICommand;
