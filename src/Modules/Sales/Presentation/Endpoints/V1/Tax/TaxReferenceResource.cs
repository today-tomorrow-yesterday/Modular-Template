using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Tax;

internal sealed class TaxReferenceResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetTaxQuestionsEndpoint(),
        new GetTaxExemptionsEndpoint()
    ];
}
