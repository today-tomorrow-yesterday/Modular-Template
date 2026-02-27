using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageWarranty;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Warranty;

internal sealed class UpdatePackageWarrantyEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/warranty", HandleAsync)
            .WithSummary("Update package warranty section")
            .WithDescription("Upserts the warranty section from a previously generated warranty quote.")
            .WithName("UpdatePackageWarranty")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PackageUpdatedResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageWarrantyRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageWarrantyCommand(
            publicPackageId,
            request.WarrantySelected,
            request.WarrantyAmount);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => ApiResponse.Ok(new PackageUpdatedResponse(
                r.GrossProfit,
                r.CommissionableGrossProfit,
                r.MustRecalculateTaxes)),
            ApiResponse.Problem);
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "warrantySelected": true,
            "warrantyAmount": 1500.00
        }
        """;
    }
}

public sealed record UpdatePackageWarrantyRequest(
    bool WarrantySelected,
    decimal WarrantyAmount);
