using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class GenerateHomeFirstQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/insurance/quote/home-first", HandleAsync)
            .WithSummary("Generate HomeFirst property/casualty insurance quote")
            .WithDescription("Calls iSeries adapter to generate a HomeFirst insurance quote based on home, delivery, and customer details.")
            .WithName("GenerateHomeFirstQuote")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<HomeFirstQuoteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "coverageAmount": 300000.00,
            "occupancyType": "P",
            "isHomeLocatedInPark": false,
            "isLandCustomerOwned": true,
            "isHomeOnPermanentFoundation": false,
            "isPremiumFinanced": true,
            "customerBirthDate": "1985-06-15",
            "coApplicantBirthDate": null,
            "mailingAddress": "5000 Clayton Rd",
            "mailingCity": "Maryville",
            "mailingState": "TN",
            "mailingZip": "37801"
        }
        """;
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        GenerateHomeFirstQuoteRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new GenerateHomeFirstQuoteCommand(
            publicSaleId,
            request.CoverageAmount,
            request.OccupancyType,
            request.IsHomeLocatedInPark,
            request.IsLandCustomerOwned,
            request.IsHomeOnPermanentFoundation,
            request.IsPremiumFinanced,
            request.CustomerBirthDate,
            request.CoApplicantBirthDate,
            request.MailingAddress,
            request.MailingCity,
            request.MailingState,
            request.MailingZip);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => ApiResponse.Ok(new HomeFirstQuoteResponse(
                r.TempLinkId,
                r.InsuranceCompanyName,
                r.Premium,
                r.CoverageAmount,
                r.MaxCoverage,
                r.IsEligible,
                r.ErrorMessage)),
            ApiResponse.Problem);
    }
}

public sealed record GenerateHomeFirstQuoteRequest(
    decimal CoverageAmount,
    char OccupancyType,
    bool IsHomeLocatedInPark,
    bool IsLandCustomerOwned,
    bool IsHomeOnPermanentFoundation,
    bool IsPremiumFinanced,
    DateTime CustomerBirthDate,
    DateTime? CoApplicantBirthDate,
    string MailingAddress,
    string MailingCity,
    string MailingState,
    string MailingZip);

public sealed record HomeFirstQuoteResponse(
    int TempLinkId,
    string InsuranceCompanyName,
    decimal Premium,
    decimal CoverageAmount,
    decimal MaxCoverage,
    bool IsEligible,
    string? ErrorMessage);
