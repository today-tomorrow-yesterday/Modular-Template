using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Commission.CalculateCommission;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Commission;

internal sealed class CalculateCommissionEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicPackageId:guid}/commission", HandleAsync)
            .WithSummary("Calculate commission for a package")
            .WithDescription("Executes iSeries commission calculation. All inputs derived server-side.")
            .WithName("CalculateCommission")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<CommissionCalculationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CalculateCommissionCommand(publicPackageId);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new CommissionCalculationResponse(
                r.CommissionableGrossProfit,
                r.TotalCommission,
                r.SplitDetails.Select(s => new CommissionSplitDetail(
                    s.EmployeeNumber,
                    s.Role,
                    s.SplitPercentage,
                    s.CommissionAmount)).ToList())),
            ApiResults.Problem);
    }
}

public sealed record CommissionCalculationResponse(
    decimal CommissionableGrossProfit,
    decimal TotalCommission,
    IReadOnlyCollection<CommissionSplitDetail> SplitDetails);

public sealed record CommissionSplitDetail(
    int EmployeeNumber,
    string Role,
    decimal SplitPercentage,
    decimal CommissionAmount);
