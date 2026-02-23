using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageTradeIns;

public sealed record UpdatePackageTradeInsCommand(
    Guid PackagePublicId,
    UpdatePackageTradeInItemRequest[] Items) : ICommand<UpdatePackageTradeInsResult>;

public sealed record UpdatePackageTradeInItemRequest(
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    string TradeType,
    int Year,
    string Make,
    string Model,
    decimal? FloorWidth,
    decimal? FloorLength,
    decimal TradeAllowance,
    decimal PayoffAmount,
    decimal BookInAmount);

public sealed record UpdatePackageTradeInsResult(
    decimal GrossProfit,
    decimal CommissionableGrossProfit,
    bool MustRecalculateTaxes);
