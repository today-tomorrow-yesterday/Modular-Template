using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Builder;
using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;

namespace Rtl.Core.Api.Diagnostics;

/// <summary>
/// Development-only diagnostic endpoints to verify iSeries adapter connectivity.
/// These are NOT mapped in non-Development environments.
/// </summary>
internal static class ISeriesDiagnosticsEndpoints
{
    internal static IEndpointRouteBuilder MapISeriesDiagnostics(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/diag/iseries")
            .WithTags("Diagnostics: iSeries")
            .AllowAnonymous();

        group.MapGet("/ping", async (IiSeriesAdapter adapter, CancellationToken ct) =>
        {
            // W&A by count is a local calculation (no HTTP call) — verifies DI wiring
            var price = await adapter.CalculateWheelAndAxlePrice(
                new WheelAndAxlePriceByCountRequest { NumberOfWheels = 4, NumberOfAxles = 2 }, ct);

            return Results.Ok(new { Test = "WheelAndAxleByCount", Price = price, Status = "OK" });
        })
        .WithName("DiagISeriesPing")
        .WithSummary("Verify iSeries adapter DI wiring (local calculation, no HTTP)");

        group.MapGet("/w-and-a/{homeCenterNumber}/{stockNumber}", async (
            int homeCenterNumber,
            string stockNumber,
            IiSeriesAdapter adapter,
            CancellationToken ct) =>
        {
            // Actually hits the iSeries gateway — verifies HTTP connectivity + auth
            var price = await adapter.CalculateWheelAndAxlePrice(
                new WheelAndAxlePriceByStockRequest
                {
                    HomeCenterNumber = homeCenterNumber,
                    StockNumber = stockNumber
                }, ct);

            return Results.Ok(new { HomeCenterNumber = homeCenterNumber, StockNumber = stockNumber, WheelAndAxlePrice = price });
        })
        .WithName("DiagISeriesWheelAndAxle")
        .WithSummary("Hit iSeries gateway for W&A price by stock (verifies HTTP + auth)");

        return app;
    }
}
