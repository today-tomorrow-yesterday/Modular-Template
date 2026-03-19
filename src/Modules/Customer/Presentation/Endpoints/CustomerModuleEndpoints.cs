using Modules.Customer.Presentation.Endpoints.V1.Customers;
using Rtl.Core.Presentation.Endpoints;

namespace Modules.Customer.Presentation.Endpoints;

public sealed class CustomerModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "customers";

    public override string ModuleName => "Customer Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield return ("customers", "Customers", new CustomersResource());
    }
}
