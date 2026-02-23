using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.Sales;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Sales.Application.Packages.GetPackagesBySale;

internal sealed class GetPackagesBySaleQueryHandler(
    ISaleRepository saleRepository,
    IPackageRepository packageRepository)
    : IQueryHandler<GetPackagesBySaleQuery, IReadOnlyCollection<PackageSummaryResponse>>
{
    public async Task<Result<IReadOnlyCollection<PackageSummaryResponse>>> Handle(
        GetPackagesBySaleQuery request,
        CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByPublicIdAsync(request.SalePublicId, cancellationToken);

        if (sale is null)
        {
            return Result.Failure<IReadOnlyCollection<PackageSummaryResponse>>(
                SaleErrors.NotFoundByPublicId(request.SalePublicId));
        }

        var packages = await packageRepository.GetBySaleIdAsync(sale.Id, cancellationToken);

        var response = packages.Select(p => new PackageSummaryResponse(
            p.PublicId,
            p.Name,
            p.Ranking,
            p.IsPrimaryPackage,
            p.Status.ToString(),
            p.GrossProfit,
            p.CommissionableGrossProfit,
            p.MustRecalculateTaxes)).ToList();

        return response;
    }
}
