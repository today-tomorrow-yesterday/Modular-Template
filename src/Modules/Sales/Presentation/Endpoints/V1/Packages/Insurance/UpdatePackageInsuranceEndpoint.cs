using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Packages.UpdatePackageInsurance;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Packages.Insurance;

internal sealed class UpdatePackageInsuranceEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPut("/{publicPackageId:guid}/insurance", HandleAsync)
            .WithSummary("Update package insurance section")
            .WithDescription("Upserts the insurance section from a previously generated quote.")
            .WithName("UpdatePackageInsurance")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PackageUpdatedResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    private static async Task<IResult> HandleAsync(
        Guid publicPackageId,
        UpdatePackageInsuranceRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new UpdatePackageInsuranceCommand(
            publicPackageId,
            request.InsuranceType,
            request.CoverageAmount,
            request.HasFoundationOrMasonry,
            request.InParkOrSubdivision,
            request.IsLandOwnedByCustomer,
            request.IsPremiumFinanced,
            request.CompanyName,
            request.MaxCoverage,
            request.TotalPremium);

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
            "insuranceType": "HomeFirst",
            "coverageAmount": 300000.00,
            "hasFoundationOrMasonry": false,
            "inParkOrSubdivision": false,
            "isLandOwnedByCustomer": true,
            "isPremiumFinanced": true,
            "companyName": "HomeFirst Insurance Co",
            "maxCoverage": 350000.00,
            "totalPremium": 1500.00
        }
        """;
    }
}

public sealed record UpdatePackageInsuranceRequest(
    decimal SalePrice,
    decimal EstimatedCost,
    decimal RetailSalePrice,
    string? Responsibility,
    bool ShouldExcludeFromPricing,
    string InsuranceType,
    decimal CoverageAmount,
    bool HasFoundationOrMasonry,
    bool InParkOrSubdivision,
    bool IsLandOwnedByCustomer,
    bool IsPremiumFinanced,
    int QuoteId,
    string CompanyName,
    decimal MaxCoverage,
    decimal TotalPremium);
