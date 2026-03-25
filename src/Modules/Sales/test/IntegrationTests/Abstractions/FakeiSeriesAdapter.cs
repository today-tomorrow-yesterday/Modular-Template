using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Commission;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Adapters.ISeries.Tax;

namespace Modules.Sales.IntegrationTests.Abstractions;

#pragma warning disable IDE1006
public sealed class FakeiSeriesAdapter : IiSeriesAdapter
#pragma warning restore IDE1006
{
    public const decimal WaSalePrice = 500m;
    public const decimal WaCost = 300m;

    public Task<WheelAndAxlePriceResult> GetWheelAndAxlePriceByStock(
        WheelAndAxlePriceByStockRequest request, CancellationToken ct)
        => Task.FromResult(new WheelAndAxlePriceResult(WaSalePrice, WaCost));

    public Task<WheelAndAxlePriceResult> CalculateWheelAndAxlePriceByCount(
        WheelAndAxlePriceByCountRequest request, CancellationToken ct)
        => Task.FromResult(new WheelAndAxlePriceResult(WaSalePrice, WaCost));

    public Task<decimal> CalculateRetailPrice(RetailPriceRequest request, CancellationToken ct)
        => Task.FromResult(0m);

    public Task<OptionTotalsResult> CalculateOptionTotals(OptionTotalsRequest request, CancellationToken ct)
        => Task.FromResult(new OptionTotalsResult { FactoryOptionTotal = 0m, RetailOptionTotal = 0m });

    public Task DeleteTaxQuestionAnswers(DeleteTaxQuestionAnswersRequest request, CancellationToken ct)
        => Task.CompletedTask;

    public Task UpdateAllowances(AllowanceUpdateRequest request, CancellationToken ct)
        => Task.CompletedTask;

    public Task InsertTaxQuestionAnswers(InsertTaxQuestionAnswersRequest request, CancellationToken ct)
        => Task.CompletedTask;

    public Task<TaxCalculationResult> CalculateTax(TaxCalculationRequest request, CancellationToken ct)
        => Task.FromResult(new TaxCalculationResult
        {
            StateTax = 1000m, CityTax = 200m, CountyTax = 150m
        });

    public Task<HomeFirstQuoteResult> CalculateHomeFirstQuote(HomeFirstQuoteRequest request, CancellationToken ct)
        => Task.FromResult(new HomeFirstQuoteResult
        {
            InsuranceCompanyName = "Test", TotalPremium = 0m, MaximumCoverage = 0m, TempLinkId = 1
        });

    public Task<WarrantyQuoteResult> CalculateWarrantyQuote(WarrantyQuoteRequest request, CancellationToken ct)
        => Task.FromResult(new WarrantyQuoteResult { Premium = 0m, SalesTaxPremium = 0m });

    public Task<CommissionResult> CalculateCommission(CommissionCalculationRequest request, CancellationToken ct)
        => Task.FromResult(new CommissionResult
        {
            CommissionableGrossProfit = 5000m, TotalCommission = 0m, EmployeeSplits = []
        });

    [Obsolete]
    public Task<string> PingHealthCheckAsync(CancellationToken ct) => Task.FromResult("OK");

    [Obsolete]
    public Task<string> PingTaxExemptionsAsync(CancellationToken ct) => Task.FromResult("OK");
}
