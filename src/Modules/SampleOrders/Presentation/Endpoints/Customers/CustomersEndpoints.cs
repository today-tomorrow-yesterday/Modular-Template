using ModularTemplate.Presentation.Endpoints;

namespace Modules.SampleOrders.Presentation.Endpoints.Customers;

internal sealed class CustomersEndpoints : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        // V1 endpoints
        new V1.GetAllCustomersEndpoint(),
        new V1.GetCustomerByIdEndpoint(),
        new V1.CreateCustomerEndpoint(),
        new V1.UpdateCustomerEndpoint()
    ];
}
