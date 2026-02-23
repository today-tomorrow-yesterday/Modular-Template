using Rtl.Core.Presentation.Endpoints;

namespace Modules.SampleSales.Presentation.Endpoints.Products;

internal sealed class ProductsEndpoints : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        // V1 endpoints
        new V1.GetAllProductsEndpoint(),
        new V1.GetProductByIdEndpoint(),
        new V1.CreateProductEndpoint(),
        new V1.UpdateProductEndpoint()
    ];
}
