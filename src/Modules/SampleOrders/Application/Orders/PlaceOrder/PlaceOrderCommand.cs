using Rtl.Core.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    int CustomerId,
    int ProductId,
    int Quantity) : ICommand<int>;
