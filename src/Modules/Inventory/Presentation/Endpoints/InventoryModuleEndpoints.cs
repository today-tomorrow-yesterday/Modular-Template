using Modules.Inventory.Presentation.Endpoints.V1.LandInventory;
using Modules.Inventory.Presentation.Endpoints.V1.OnLotInventory;
using Modules.Inventory.Presentation.Endpoints.V1.RepoInventory;
using Modules.Inventory.Presentation.Endpoints.V1.Transportation;
using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints;

public sealed class InventoryModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "inventory";

    public override string ModuleName => "Inventory Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield return ("inventory/on-lot", "On-Lot Inventory", new OnLotInventoryResource());
        yield return ("inventory/land", "Land Inventory", new LandInventoryResource());
        yield return ("inventory/repo", "Repo Inventory", new RepoInventoryResource());
        yield return ("inventory/transportation", "Transportation", new TransportationResource());
    }
}
