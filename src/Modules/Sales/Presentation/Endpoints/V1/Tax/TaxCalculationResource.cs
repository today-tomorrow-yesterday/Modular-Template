using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class TaxCalculationResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new UpdatePackageTaxEndpoint(),
        new CalculateTaxesEndpoint()
    ];
}
