using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Insurance.RecordOutsideInsurance;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class RecordOutsideInsuranceEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/insurance/quote/outside", HandleAsync)
            .WithSummary("Record third-party (outside) insurance")
            .WithDescription("Records insurance obtained from a third-party provider. No iSeries call — simply stores the provider name, coverage, and premium on the package.")
            .WithName("RecordOutsideInsurance")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<object>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "providerName": "State Farm",
            "coverageAmount": 300000.00,
            "premiumAmount": 1500.00
        }
        """;
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        RecordOutsideInsuranceRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new RecordOutsideInsuranceCommand(
            publicSaleId,
            request.ProviderName,
            request.CoverageAmount,
            request.PremiumAmount);

        var result = await sender.Send(command, ct);

        return result.Match(
            () => ApiResponse.Success(),
            ApiResponse.Problem);
    }
}

public sealed record RecordOutsideInsuranceRequest(
    string ProviderName,
    decimal CoverageAmount,
    decimal PremiumAmount);
