using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Commission.CalculateCommission;

public sealed record CalculateCommissionCommand(
    Guid PackagePublicId) : ICommand<CalculateCommissionResult>;

public sealed record CalculateCommissionResult(
    int PackageId,
    decimal CommissionableGrossProfit,
    decimal TotalCommission,
    IReadOnlyCollection<CommissionSplitResult> SplitDetails);

public sealed record CommissionSplitResult(
    int EmployeeNumber,
    string Role,
    decimal SplitPercentage,
    decimal CommissionRatePercentage,
    decimal CommissionAmount);
