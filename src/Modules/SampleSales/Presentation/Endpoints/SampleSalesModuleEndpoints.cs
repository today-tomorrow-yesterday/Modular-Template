using Modules.SampleSales.Presentation.Endpoints.Catalogs;
using Modules.SampleSales.Presentation.Endpoints.Products;
using ModularTemplate.Presentation.Endpoints;

namespace Modules.SampleSales.Presentation.Endpoints;

public sealed class SampleSalesModuleEndpoints : ModuleEndpoints
{
    public override string ModulePrefix => "samplesales";

    public override string ModuleName => "SampleSales Module";

    protected override IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources()
    {
        yield return ("products", "Products", new ProductsEndpoints());
        yield return ("catalogs", "Catalogs", new CatalogsEndpoints());
    }
}
