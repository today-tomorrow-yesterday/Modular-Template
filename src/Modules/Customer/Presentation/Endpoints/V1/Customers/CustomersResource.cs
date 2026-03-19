using Rtl.Core.Presentation.Endpoints;

namespace Modules.Customer.Presentation.Endpoints.V1.Customers;

internal sealed class CustomersResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetCustomerByIdEndpoint()    ];
}
