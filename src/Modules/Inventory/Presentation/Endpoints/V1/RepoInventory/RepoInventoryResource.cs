using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints.V1.RepoInventory;

internal sealed class RepoInventoryResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetRepoInventoryEndpoint()
    ];
}
