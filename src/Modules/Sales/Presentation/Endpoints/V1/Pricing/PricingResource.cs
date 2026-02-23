using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Pricing;

internal sealed class PricingResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetRetailPriceEndpoint(),
        new GetHomeMultipliersEndpoint(),
        new GetOptionTotalsEndpoint(),
        new GetWheelsAndAxlesPriceEndpoint(),
        new GetWheelsAndAxlesPriceByStockEndpoint()
    ];
}
