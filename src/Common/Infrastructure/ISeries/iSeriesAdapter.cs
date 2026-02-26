using Microsoft.Extensions.Logging;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Commission;
using Rtl.Core.Application.Adapters.ISeries.Insurance;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Infrastructure.ISeries.Mapping;
using Rtl.Core.Infrastructure.ISeries.WireModels;
using Rtl.Core.Infrastructure.ISeries.WireModels.Commission;
using Rtl.Core.Infrastructure.ISeries.WireModels.Insurance;
using Rtl.Core.Infrastructure.ISeries.WireModels.Pricing;
using Rtl.Core.Infrastructure.ISeries.WireModels.Tax;
using System.Net.Http.Json;
using System.Text.Json;

namespace Rtl.Core.Infrastructure.ISeries;

#pragma warning disable IDE1006 // Naming Styles
internal sealed class iSeriesAdapter(
    HttpClient httpClient,
    ILogger<iSeriesAdapter> logger) : IiSeriesAdapter
#pragma warning restore IDE1006 // Naming Styles
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    // Both methods below call iSeries for W&A pricing — they hit different stored procedures:
    //   By stock → "home-inventory-ancillary-data" (legacy iSeries name; returns OData with wheelAndAxlePrice)
    //   By count → "wheels-and-axles/price"         (returns salePrice + cost)

    public async Task<WheelAndAxlePriceResult> GetWheelAndAxlePriceByStock(
        WheelAndAxlePriceByStockRequest request, CancellationToken ct)
    {
        // The path looks unrelated to W&A, but "ancillary data" is the iSeries term for this lookup.
        // This is the iSeries endpoint name — we cannot rename it.
        var url = $"v1/inventory/home-inventory-ancillary-data?homeCenterNumber={request.HomeCenterNumber}&stockNumbers={request.StockNumber}";
        logger.LogDebug("GET {Url}", url);

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var odata = await response.Content.ReadFromJsonAsync<ODataResponse<WheelAndAxlePriceWireResponse>>(JsonOptions, ct);
        var price = odata?.Values.FirstOrDefault()?.WheelAndAxlePrice ?? 0m;
        // ByStock wire returns a single price — use it for both SalePrice and Cost
        return new WheelAndAxlePriceResult(price, price);
    }

    public async Task<WheelAndAxlePriceResult> CalculateWheelAndAxlePriceByCount(
        WheelAndAxlePriceByCountRequest request, CancellationToken ct)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var url = $"v1/wheels-and-axles/price?date={date}&numberOfWheels={request.NumberOfWheels}&numberOfAxles={request.NumberOfAxles}";
        logger.LogDebug("GET {Url}", url);

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<WheelAndAxlePriceByCountWireResponse>(JsonOptions, ct);
        return new WheelAndAxlePriceResult(result?.SalePrice ?? 0m, result?.Cost ?? 0m);
    }

    public async Task<decimal> CalculateRetailPrice(
        RetailPriceRequest request, CancellationToken ct)
    {
        var response = await PostAsync<RetailPriceWireRequest, RetailPriceWireResponse>(
            "v1/pricing/retail", request.ToWire(), ct);
        return response.RetailPrice;
    }

    public async Task<OptionTotalsResult> CalculateOptionTotals(
        OptionTotalsRequest request, CancellationToken ct)
    {
        var response = await PostAsync<OptionTotalsWireRequest, OptionTotalsWireResponse>(
            "v1/pricing/option-totals", request.ToWire(), ct);
        return response.ToDomain();
    }

    public async Task DeleteTaxQuestionAnswers(
        DeleteTaxQuestionAnswersRequest request, CancellationToken ct)
    {
        var url = $"v1/taxes/questions/delete?appId={request.AppId}";
        logger.LogDebug("POST {Url}", url);
        var response = await httpClient.PostAsync(url, null, ct);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAllowances(AllowanceUpdateRequest request, CancellationToken ct)
        => await PostAsync("v1/taxes/allowances", request.ToWire(), ct);

    public async Task InsertTaxQuestionAnswers(
        InsertTaxQuestionAnswersRequest request, CancellationToken ct)
        => await PostAsync("v1/taxes/questions/insert", request.ToWire(), ct);

    public async Task<TaxCalculationResult> CalculateTax(
        TaxCalculationRequest request, CancellationToken ct)
    {
        var response = await PostAsync<TaxCalcWireRequest, TaxCalcWireResponse>(
            "v1/taxes", request.ToWire(), ct);
        return response.ToDomain();
    }

    public async Task<HomeFirstQuoteResult> CalculateHomeFirstQuote(
        HomeFirstQuoteRequest request, CancellationToken ct)
    {
        var response = await PostAsync<HomeFirstWireRequest, HomeFirstWireResponse>(
            "v1/insurance/home-first-quote", request.ToWire(), ct);
        return response.ToDomain();
    }

    public async Task<WarrantyQuoteResult> CalculateWarrantyQuote(
        WarrantyQuoteRequest request, CancellationToken ct)
    {
        var response = await PostAsync<WarrantyWireRequest, WarrantyWireResponse>(
            "v1/insurance/hbpp-quote", request.ToWire(), ct);
        return response.ToDomain();
    }

    public async Task<CommissionResult> CalculateCommission(
        CommissionCalculationRequest request, CancellationToken ct)
    {
        var response = await PostAsync<CommissionWireRequest, CommissionWireResponse>(
            "v1/commissions", request.ToWire(), ct);
        return response.ToDomain();
    }

    #pragma warning disable CS0618 // Obsolete
    public async Task<string> PingHealthCheckAsync(CancellationToken ct)
    {
        logger.LogDebug("GET v1/health-check (diag)");
        var response = await httpClient.GetAsync("v1/health-check?echoMessage=diag-ping", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<string> PingTaxExemptionsAsync(CancellationToken ct)
    {
        logger.LogDebug("GET v1/taxes/exemptions (diag)");
        var response = await httpClient.GetAsync("v1/taxes/exemptions", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }
    #pragma warning restore CS0618

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path, TRequest body, CancellationToken ct)
    {
        logger.LogDebug("POST {Path}", path);
        var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, ct)
            ?? throw new InvalidOperationException($"iSeries returned null response from {path}");
    }

    private async Task PostAsync<TRequest>(string path, TRequest body, CancellationToken ct)
    {
        logger.LogDebug("POST {Path}", path);
        var response = await httpClient.PostAsJsonAsync(path, body, JsonOptions, ct);
        response.EnsureSuccessStatusCode();
    }
}
