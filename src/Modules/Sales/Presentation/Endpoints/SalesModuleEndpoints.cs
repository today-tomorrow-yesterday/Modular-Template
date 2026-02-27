using Modules.Sales.Presentation.Endpoints.V1.Commission;
using Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;
using Modules.Sales.Presentation.Endpoints.V1.Insurance;
using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Modules.Sales.Presentation.Endpoints.V1.Pricing;
using Modules.Sales.Presentation.Endpoints.V1.ProjectCosts;
using Modules.Sales.Presentation.Endpoints.V1.Sales;
using Modules.Sales.Presentation.Endpoints.V1.Tax;
using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints;

public sealed class SalesModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "sales";

    public override string ModuleName => "Sales Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield return ("sales", "Sales", new SalesResource());
        yield return ("sales", "Pricing", new PricingResource());
        yield return ("sales", "Insurance", new InsuranceResource());
        yield return ("sales", "Delivery Address", new DeliveryAddressResource());
        yield return ("sales", "Tax", new TaxReferenceResource());
        yield return ("sales", "Project Costs", new ProjectCostsResource());
        yield return ("packages", "Packages", new PackagesResource());
        yield return ("packages", "Tax", new TaxCalculationResource());
        yield return ("packages", "Commission", new CommissionResource());
    }
}
