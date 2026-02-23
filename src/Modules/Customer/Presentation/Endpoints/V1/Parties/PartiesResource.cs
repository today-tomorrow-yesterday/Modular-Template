using Rtl.Core.Presentation.Endpoints;

namespace Modules.Customer.Presentation.Endpoints.V1.Parties;

internal sealed class PartiesResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetPartyByIdEndpoint(),
        new SearchPartyByCrmIdEndpoint()
    ];
}
