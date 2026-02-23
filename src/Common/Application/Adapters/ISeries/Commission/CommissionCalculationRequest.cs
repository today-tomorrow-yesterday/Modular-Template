namespace Rtl.Core.Application.Adapters.ISeries.Commission;

public sealed class CommissionCalculationRequest
{
    public int AppId { get; init; }
    public decimal Cost { get; init; }
    public decimal LandPayoff { get; init; }
    public decimal LandImprovements { get; init; }
    public decimal AdjustedCost { get; init; }
    public int EmployeeNumber { get; init; }
    public HomeCondition HomeCondition { get; init; }
    public int HomeCenterNumber { get; init; }
    public CommissionSplit[] Splits { get; init; } = [];
}

public sealed class CommissionSplit
{
    public int EmployeeNumber { get; init; }
    public decimal PayPercentage { get; init; }
    public decimal GrossPayPercentage { get; init; }
    public decimal? TotalCommissionRate { get; init; }
}
