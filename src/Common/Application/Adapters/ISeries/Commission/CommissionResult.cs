namespace Rtl.Core.Application.Adapters.ISeries.Commission;

public sealed class CommissionResult
{
    public decimal CommissionableGrossProfit { get; init; }
    public decimal TotalCommission { get; init; }
    public CommissionSplitResult[] EmployeeSplits { get; init; } = [];
}

public sealed class CommissionSplitResult
{
    public int EmployeeNumber { get; init; }
    public decimal Pay { get; init; }
    public decimal GrossPayPercentage { get; init; }
    public decimal? TotalCommissionRate { get; init; }
}
