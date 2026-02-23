using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class InsuranceResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new InsuranceQuoteEndpoint()
    ];
}
