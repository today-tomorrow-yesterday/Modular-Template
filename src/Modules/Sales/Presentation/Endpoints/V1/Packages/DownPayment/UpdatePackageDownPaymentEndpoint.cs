using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageDownPayment;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.DownPayment;

internal sealed class UpdatePackageDownPaymentEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/down-payment", HandleAsync)
            .WithSummary("Update package down payment section")
            .WithDescription("Upserts the down payment amount. 0 removes existing line.")
            .WithName("UpdatePackageDownPayment")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PackageUpdatedResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageDownPaymentRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageDownPaymentCommand(publicPackageId, request.Amount);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResults.Problem);
    }
}

public sealed record UpdatePackageDownPaymentRequest(decimal Amount);
