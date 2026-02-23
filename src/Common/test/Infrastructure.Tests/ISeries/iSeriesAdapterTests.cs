using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Commission;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using Rtl.Core.Application.Adapters.ISeries.Tax;
using Rtl.Core.Infrastructure.ISeries;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.ISeries;

#pragma warning disable IDE1006 // Naming Styles
public class iSeriesAdapterTests
#pragma warning restore IDE1006
{
    private readonly FakeMessageHandler _handler = new();

    private iSeriesAdapter CreateSut()
    {
        var client = new HttpClient(_handler)
        {
            BaseAddress = new Uri("https://test.example.com/api/")
        };
        return new iSeriesAdapter(client, NullLogger<iSeriesAdapter>.Instance);
    }

    private void SetJsonResponse(string json)
    {
        _handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    // --- Wheel & Axle by Count (local calculation, no HTTP) ---

    [Fact]
    public async Task CalculateWheelAndAxlePrice_ByCount_Returns_Correct_Total()
    {
        var sut = CreateSut();
        var request = new WheelAndAxlePriceByCountRequest { NumberOfWheels = 4, NumberOfAxles = 2 };

        var result = await sut.CalculateWheelAndAxlePrice(request, CancellationToken.None);

        Assert.Equal(900m, result); // 4 * $100 + 2 * $250
        Assert.Equal(0, _handler.CallCount); // no HTTP call
    }

    [Fact]
    public async Task CalculateWheelAndAxlePrice_ByCount_Zero_Returns_Zero()
    {
        var sut = CreateSut();
        var request = new WheelAndAxlePriceByCountRequest { NumberOfWheels = 0, NumberOfAxles = 0 };

        var result = await sut.CalculateWheelAndAxlePrice(request, CancellationToken.None);

        Assert.Equal(0m, result);
    }

    // --- Wheel & Axle by Stock (OData GET) ---

    [Fact]
    public async Task CalculateWheelAndAxlePrice_ByStock_Sends_Get_With_Correct_QueryParams()
    {
        SetJsonResponse("""{"$values": [{"wheelAndAxlePrice": 575.00}]}""");
        var sut = CreateSut();
        var request = new WheelAndAxlePriceByStockRequest { HomeCenterNumber = 42, StockNumber = "ABC123" };

        var result = await sut.CalculateWheelAndAxlePrice(request, CancellationToken.None);

        Assert.Equal(575m, result);
        Assert.Equal(HttpMethod.Get, _handler.LastRequest!.Method);
        Assert.Contains("homeCenterNumber=42", _handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("stockNumbers=ABC123", _handler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task CalculateWheelAndAxlePrice_ByStock_Empty_Values_Returns_Zero()
    {
        SetJsonResponse("""{"$values": []}""");
        var sut = CreateSut();
        var request = new WheelAndAxlePriceByStockRequest { HomeCenterNumber = 1, StockNumber = "X" };

        var result = await sut.CalculateWheelAndAxlePrice(request, CancellationToken.None);

        Assert.Equal(0m, result);
    }

    // --- Retail Price (POST) ---

    [Fact]
    public async Task CalculateRetailPrice_Posts_To_Correct_Url_And_Returns_Price()
    {
        SetJsonResponse("""{"retailPrice": 15000.50}""");
        var sut = CreateSut();
        var request = new RetailPriceRequest
        {
            HomeCenterState = "OH",
            EffectiveDate = new DateOnly(2026, 1, 15),
            SerialNumber = "SN001",
            InvoiceTotalAmount = 50000m,
            NumberOfAxles = 2,
            FactoryOptionTotal = 1000m,
            RetailOptionTotal = 2000m,
            ModelNumber = "MOD-1",
            BaseCost = 40000m
        };

        var result = await sut.CalculateRetailPrice(request, CancellationToken.None);

        Assert.Equal(15000.50m, result);
        Assert.Equal(HttpMethod.Post, _handler.LastRequest!.Method);
        Assert.EndsWith("v1/pricing/retail", _handler.LastRequest.RequestUri!.AbsolutePath);
    }

    // --- Option Totals (POST — verifies HbgOptionTotal → FactoryOptionTotal mapping) ---

    [Fact]
    public async Task CalculateOptionTotals_Maps_HbgOptionTotal_To_FactoryOptionTotal()
    {
        SetJsonResponse("""{"hbgOptionTotal": 3500.00, "retailOptionTotal": 4200.00}""");
        var sut = CreateSut();
        var request = new OptionTotalsRequest
        {
            HomeCenterState = "OH",
            EffectiveDate = new DateOnly(2026, 2, 1),
            PlantNumber = 1,
            QuoteNumber = 100,
            OrderNumber = 200
        };

        var result = await sut.CalculateOptionTotals(request, CancellationToken.None);

        Assert.Equal(3500m, result.FactoryOptionTotal);
        Assert.Equal(4200m, result.RetailOptionTotal);
        Assert.Equal(HttpMethod.Post, _handler.LastRequest!.Method);
        Assert.EndsWith("v1/pricing/option-totals", _handler.LastRequest.RequestUri!.AbsolutePath);
    }

    // --- Delete Tax Question Answers (POST with null body, appId in query) ---

    [Fact]
    public async Task DeleteTaxQuestionAnswers_Posts_To_Correct_Url_With_AppId()
    {
        var sut = CreateSut();
        var request = new DeleteTaxQuestionAnswersRequest { AppId = 12345 };

        await sut.DeleteTaxQuestionAnswers(request, CancellationToken.None);

        Assert.Equal(HttpMethod.Post, _handler.LastRequest!.Method);
        Assert.Contains("appId=12345", _handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("v1/taxes/questions/delete", _handler.LastRequest.RequestUri.ToString());
    }

    // --- Commission (POST — verifies complex response mapping with splits) ---

    [Fact]
    public async Task CalculateCommission_Maps_Response_With_Splits()
    {
        SetJsonResponse("""
            {
                "commissionableGrossProfit": 25000.00,
                "totalCommission": 5000.00,
                "employeeSplits": [
                    {"employeeNumber": 101, "pay": 3000.00, "grossPayPercentage": 60.0, "totalCommissionRate": 20.0},
                    {"employeeNumber": 102, "pay": 2000.00, "grossPayPercentage": 40.0, "totalCommissionRate": null}
                ]
            }
            """);
        var sut = CreateSut();
        var request = new CommissionCalculationRequest
        {
            AppId = 1,
            Cost = 100000m,
            HomeCondition = HomeCondition.New,
            HomeCenterNumber = 42,
            Splits =
            [
                new CommissionSplit { EmployeeNumber = 101, PayPercentage = 60, GrossPayPercentage = 60 }
            ]
        };

        var result = await sut.CalculateCommission(request, CancellationToken.None);

        Assert.Equal(25000m, result.CommissionableGrossProfit);
        Assert.Equal(5000m, result.TotalCommission);
        Assert.Equal(2, result.EmployeeSplits.Length);
        Assert.Equal(101, result.EmployeeSplits[0].EmployeeNumber);
        Assert.Equal(3000m, result.EmployeeSplits[0].Pay);
        Assert.Null(result.EmployeeSplits[1].TotalCommissionRate);
    }

    // --- Diagnostic Ping endpoints ---

#pragma warning disable CS0618 // Obsolete — intentional use for testing
    [Fact]
    public async Task PingHealthCheck_Gets_Correct_Url_Returns_Body()
    {
        _handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("healthy", Encoding.UTF8, "text/plain")
        };
        var sut = CreateSut();

        var result = await sut.PingHealthCheckAsync(CancellationToken.None);

        Assert.Equal("healthy", result);
        Assert.Equal(HttpMethod.Get, _handler.LastRequest!.Method);
        Assert.Contains("v1/health-check", _handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("echoMessage=diag-ping", _handler.LastRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task PingTaxExemptions_Gets_Correct_Url_Returns_Body()
    {
        _handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""[{"code":"EX1"}]""", Encoding.UTF8, "application/json")
        };
        var sut = CreateSut();

        var result = await sut.PingTaxExemptionsAsync(CancellationToken.None);

        Assert.Contains("EX1", result);
        Assert.Equal(HttpMethod.Get, _handler.LastRequest!.Method);
        Assert.Contains("v1/taxes/exemptions", _handler.LastRequest.RequestUri!.ToString());
    }
#pragma warning restore CS0618

    // --- Error handling ---

    [Fact]
    public async Task Post_NonSuccess_StatusCode_Throws_HttpRequestException()
    {
        _handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var sut = CreateSut();
        var request = new RetailPriceRequest
        {
            HomeCenterState = "OH",
            SerialNumber = "X",
            ModelNumber = "M"
        };

        await Assert.ThrowsAsync<HttpRequestException>(
            () => sut.CalculateRetailPrice(request, CancellationToken.None));
    }

    [Fact]
    public async Task Get_NonSuccess_StatusCode_Throws_HttpRequestException()
    {
        _handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.BadGateway);
        var sut = CreateSut();
        var request = new WheelAndAxlePriceByStockRequest { HomeCenterNumber = 1, StockNumber = "X" };

        await Assert.ThrowsAsync<HttpRequestException>(
            () => sut.CalculateWheelAndAxlePrice(request, CancellationToken.None));
    }
}
