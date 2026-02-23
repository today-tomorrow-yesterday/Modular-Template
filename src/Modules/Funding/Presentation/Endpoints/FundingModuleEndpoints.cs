using Rtl.Core.Presentation.Endpoints;

namespace Modules.Funding.Presentation.Endpoints;

public sealed class FundingModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "funding";

    public override string ModuleName => "Funding Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield break;
    }
}
