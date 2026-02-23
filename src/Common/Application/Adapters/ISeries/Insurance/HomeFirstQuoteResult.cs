namespace Rtl.Core.Application.Adapters.ISeries.Insurance;

public sealed class HomeFirstQuoteResult
{
    public string InsuranceCompanyName { get; init; } = string.Empty;
    public decimal TotalPremium { get; init; }
    public decimal MaximumCoverage { get; init; }
    public int TempLinkId { get; init; }
    public string? ErrorMessage { get; init; }
}
