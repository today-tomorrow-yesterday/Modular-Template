using Modules.Sales.Presentation.Endpoints.V1.Packages;
using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Sales;

internal sealed class SalesResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new CreateSaleEndpoint(),
        new GetSaleByIdEndpoint(),
        new GetPackagesBySaleEndpoint(),
        new CreatePackageEndpoint()
    ];
}
