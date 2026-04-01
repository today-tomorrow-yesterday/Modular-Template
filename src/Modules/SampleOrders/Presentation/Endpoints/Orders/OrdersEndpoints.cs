using ModularTemplate.Presentation.Endpoints;

namespace Modules.SampleOrders.Presentation.Endpoints.Orders;

internal sealed class OrdersEndpoints : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        // V1 endpoints
        new V1.GetAllOrdersEndpoint(),
        new V1.GetOrderByIdEndpoint(),
        new V1.PlaceOrderEndpoint(),
        new V1.UpdateOrderStatusEndpoint()
    ];
}
