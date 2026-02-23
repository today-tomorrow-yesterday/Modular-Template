using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Commission;

internal sealed class CommissionResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new CalculateCommissionEndpoint()
    ];
}
