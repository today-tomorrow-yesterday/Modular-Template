using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints.V1.Transportation;

internal sealed class TransportationResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetTransportationEndpoint()
    ];
}
