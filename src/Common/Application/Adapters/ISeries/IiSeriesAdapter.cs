using Rtl.Core.Application.Adapters.ISeries.Commission;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Adapters.ISeries.Tax;

namespace Rtl.Core.Application.Adapters.ISeries;

// Infrastructure adapter translates these domain-friendly types to iSeries wire format.
#pragma warning disable IDE1006 // Naming Styles
public interface IiSeriesAdapter
#pragma warning restore IDE1006 // Naming Styles
{
    // Pricing
    Task<decimal> GetWheelAndAxlePriceByStock(WheelAndAxlePriceByStockRequest request, CancellationToken ct);
    Task<decimal> CalculateWheelAndAxlePriceByCount(WheelAndAxlePriceByCountRequest request, CancellationToken ct);
    Task<decimal> CalculateRetailPrice(RetailPriceRequest request, CancellationToken ct);
    Task<OptionTotalsResult> CalculateOptionTotals(OptionTotalsRequest request, CancellationToken ct);

    // Tax — 4-step call sequence: Delete → Parallel[UpdateAllowances + InsertQuestionAnswers] → CalculateTax
    Task DeleteTaxQuestionAnswers(DeleteTaxQuestionAnswersRequest request, CancellationToken ct);
    Task UpdateAllowances(AllowanceUpdateRequest request, CancellationToken ct);
    Task InsertTaxQuestionAnswers(InsertTaxQuestionAnswersRequest request, CancellationToken ct);
    Task<TaxCalculationResult> CalculateTax(TaxCalculationRequest request, CancellationToken ct);

    // Insurance
    Task<HomeFirstQuoteResult> CalculateHomeFirstQuote(HomeFirstQuoteRequest request, CancellationToken ct);
    Task<WarrantyQuoteResult> CalculateWarrantyQuote(WarrantyQuoteRequest request, CancellationToken ct);

    // Commission — sequential: UpdateAllowances then CalculateCommission
    Task<CommissionResult> CalculateCommission(CommissionCalculationRequest request, CancellationToken ct);

    // Diagnostics — for testing auth/connectivity only. Do not use in production code.
    [Obsolete("Testing auth only — do not use in production code.")]
    Task<string> PingHealthCheckAsync(CancellationToken ct);

    [Obsolete("Testing auth only — do not use in production code.")]
    Task<string> PingTaxExemptionsAsync(CancellationToken ct);
}
