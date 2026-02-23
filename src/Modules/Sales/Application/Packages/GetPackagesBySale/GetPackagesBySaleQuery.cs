using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.GetPackagesBySale;

public sealed record GetPackagesBySaleQuery(Guid SalePublicId) : IQuery<IReadOnlyCollection<PackageSummaryResponse>>;

public sealed record PackageSummaryResponse(
    Guid Id,
    string Name,
    int Ranking,
    bool IsPrimaryPackage,
    string Status,
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);
