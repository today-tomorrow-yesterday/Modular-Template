using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints.V1.OnLotInventory;

internal sealed class OnLotInventoryResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetOnLotInventoryEndpoint()
    ];
}
