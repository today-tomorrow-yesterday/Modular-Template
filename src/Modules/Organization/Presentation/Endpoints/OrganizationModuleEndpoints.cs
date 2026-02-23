using Rtl.Core.Presentation.Endpoints;

namespace Modules.Organization.Presentation.Endpoints;

public sealed class OrganizationModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "organizations";

    public override string ModuleName => "Organization Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield break;
    }
}
