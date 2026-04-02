using ModularTemplate.Application.Messaging;

namespace Modules.SampleOrders.Application.Orders.PlaceOrder;

public sealed record PlaceOrderCommand(
    Guid PublicCustomerId,
    int ProductCacheId,
    int Quantity) : ICommand<Guid>;
