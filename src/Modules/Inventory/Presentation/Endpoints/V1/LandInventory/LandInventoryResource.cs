using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints.V1.LandInventory;

internal sealed class LandInventoryResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetLandInventoryEndpoint()
    ];
}
