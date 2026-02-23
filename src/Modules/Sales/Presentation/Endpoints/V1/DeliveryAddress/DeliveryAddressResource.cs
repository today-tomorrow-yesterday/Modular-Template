using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;

internal sealed class DeliveryAddressResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetDeliveryAddressEndpoint(),
        new CreateDeliveryAddressEndpoint(),
        new UpdateDeliveryAddressEndpoint()
    ];
}
