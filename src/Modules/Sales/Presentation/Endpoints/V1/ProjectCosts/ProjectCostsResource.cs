using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.ProjectCosts;

internal sealed class ProjectCostsResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new GetProjectCostCategoriesEndpoint()
    ];
}
