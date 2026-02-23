using Rtl.Core.Presentation.Endpoints;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages;

internal sealed class PackagesResource : ResourceEndpoints
{
    protected override IEndpoint[] Endpoints =>
    [
        new Home.UpdatePackageHomeEndpoint(),
        new Land.UpdatePackageLandEndpoint(),
        new TradeIns.UpdatePackageTradeInsEndpoint(),
        new DownPayment.UpdatePackageDownPaymentEndpoint(),
        new Concessions.UpdatePackageConcessionsEndpoint(),
        new Insurance.UpdatePackageInsuranceEndpoint(),
        new Warranty.UpdatePackageWarrantyEndpoint(),
        new ProjectCosts.UpdatePackageProjectCostsEndpoint(),
        new SalesTeam.UpdatePackageSalesTeamEndpoint(),
        new UpdatePackageNameEndpoint(),
        new SetPackageAsPrimaryEndpoint(),
        new DeletePackageEndpoint()
    ];

    internal sealed class Queries : ResourceEndpoints
    {
        protected override IEndpoint[] Endpoints =>
        [
            new GetPackagesBySaleEndpoint(),
            new GetPackageByIdEndpoint(),
            new CreatePackageEndpoint()
        ];
    }
}
