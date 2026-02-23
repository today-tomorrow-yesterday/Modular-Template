using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageProjectCosts;

public sealed record UpdatePackageProjectCostsCommand(
    Guid PackagePublicId,
    UpdateProjectCostItemRequest[] Items) : ICommand<UpdatePackageProjectCostsResult>;

public sealed record UpdateProjectCostItemRequest(
    int CategoryId,
    int ItemId,
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    bool ShouldExcludeFromPricing,
    string? Responsibility = null);

public sealed record UpdatePackageProjectCostsResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);
