using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageTradeIns;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.TradeIns;

internal sealed class UpdatePackageTradeInsEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/trade-ins", HandleAsync)
            .WithSummary("Update package trade-ins section")
            .WithDescription("Replaces the trade-in collection. PUT semantics — always replaces all existing trade-in lines. Empty array removes all trade-ins.")
            .WithName("UpdatePackageTradeIns")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageTradeInItem[] items,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageTradeInsCommand(
            publicPackageId,
            items.Select(i => new UpdatePackageTradeInItemRequest(
                i.SalePrice,
                i.EstimatedCost,
                i.RetailSalePrice,
                i.TradeType,
                i.Year,
                i.Make,
                i.Model,
                i.FloorWidth,
                i.FloorLength,
                i.TradeAllowance,
                i.PayoffAmount,
                i.BookInAmount)).ToArray());

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        [
            {
                "salePrice": 15000.00,
                "estimatedCost": 0.00,
                "retailSalePrice": 0.00,
                "tradeType": "Home",
                "year": 2015,
                "make": "Clayton",
                "model": "Freedom",
                "floorWidth": 14.0,
                "floorLength": 70.0,
                "tradeAllowance": 15000.00,
                "payoffAmount": 5000.00,
                "bookInAmount": 12000.00
            }
        ]
        """;
    }
}

public sealed record UpdatePackageTradeInItem(
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    string TradeType,
    int Year,
    string Make,
    string Model,
    decimal? FloorWidth,
    decimal? FloorLength,
    decimal TradeAllowance,
    decimal PayoffAmount,
    decimal BookInAmount);
