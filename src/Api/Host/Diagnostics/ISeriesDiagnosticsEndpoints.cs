using Rtl.Core.Application.Adapters.ISeries;
using Rtl.Core.Application.Adapters.ISeries.Pricing;
using System.Diagnostics;

namespace Rtl.Core.Api.Diagnostics;

/// <summary>
/// Development-only diagnostic endpoints to verify iSeries adapter connectivity.
/// Compiled out of Release builds via #if DEBUG in Program.cs.
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
            var result = await adapter.CalculateWheelAndAxlePriceByCount(
                new WheelAndAxlePriceByCountRequest { NumberOfWheels = 4, NumberOfAxles = 2 }, ct);

            return Results.Ok(new { Test = "WheelAndAxleByCount", Price = result.SalePrice, Status = "OK" });
        })
        .WithName("DiagISeriesPing")
        .WithSummary("W&A by-count pricing via iSeries — verifies HTTP + auth");

#pragma warning disable CS0618 // Obsolete — intentional use for diagnostics
        group.MapGet("/health-check", async (IiSeriesAdapter adapter, CancellationToken ct) =>
        {
            var sw = Stopwatch.StartNew();
            var body = await adapter.PingHealthCheckAsync(ct);
            sw.Stop();
            return Results.Ok(new { Endpoint = "v1/health-check", ElapsedMs = sw.ElapsedMilliseconds, Response = body });
        })
        .WithName("DiagISeriesHealthCheck")
        .WithSummary("Hit iSeries gateway health-check — verifies HTTP + auth");

        group.MapGet("/tax-exemptions", async (IiSeriesAdapter adapter, CancellationToken ct) =>
        {
            var sw = Stopwatch.StartNew();
            var body = await adapter.PingTaxExemptionsAsync(ct);
            sw.Stop();
            return Results.Ok(new { Endpoint = "v1/taxes/exemptions", ElapsedMs = sw.ElapsedMilliseconds, Response = body });
        })
        .WithName("DiagISeriesTaxExemptions")
        .WithSummary("GET tax exemption codes — verifies HTTP + auth + real data");
#pragma warning restore CS0618

        return app;
    }
}
