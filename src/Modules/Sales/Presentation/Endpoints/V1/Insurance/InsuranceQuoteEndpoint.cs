using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.Insurance.GenerateHomeFirstQuote;
using Modules.Sales.Application.Insurance.GenerateWarrantyQuote;
using Modules.Sales.Application.Insurance.RecordOutsideInsurance;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class InsuranceQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/insurance/quote", HandleAsync)
            .WithSummary("Insurance quote operations")
            .WithDescription(
                "Dispatches by `type` query parameter:\n\n" +
                "- `?type=home-first` — Generate HomeFirst property/casualty quote (11-field body → iSeries adapter)\n" +
                "- `?type=warranty` — Generate warranty extended warranty quote (no body → iSeries adapter)\n" +
                "- `?type=outside` — Record third-party insurance (3-field body, no iSeries call) → 201\n" +
                "- `?type=print` — Print HomeFirst quote sheet PDF (10-field body → iSeries adapter)")
            .WithName("InsuranceQuote")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<HomeFirstQuoteResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static Task<IResult> HandleAsync(
        Guid publicSaleId,
        string type,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        return type switch
        {
            "home-first" => HandleHomeFirstAsync(publicSaleId, httpContext, sender, ct),

            "warranty" => HandleWarrantyAsync(publicSaleId, sender, ct),

            "outside" => HandleOutsideAsync(publicSaleId, httpContext, sender, ct),

            "print" => Task.FromResult(Results.Ok(new PrintInsuranceQuoteResponse(
                FileBase64: "JVBERi0xLjQg...",
                FileName: "HomeFirst_Quote_4523.pdf",
                ContentType: "application/pdf"))),

            _ => Task.FromResult(Results.Problem(
                $"Unknown insurance quote type: '{type}'. Valid values: home-first, warranty, outside, print.",
                statusCode: StatusCodes.Status400BadRequest))
        };
    }

    private static async Task<IResult> HandleHomeFirstAsync(
        Guid publicSaleId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var request = await httpContext.Request.ReadFromJsonAsync<GenerateHomeFirstQuoteRequest>(ct);
        if (request is null)
            return Results.BadRequest("Request body is required for HomeFirst insurance quote.");

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
            r => Results.Ok(new HomeFirstQuoteResponse(
                r.TempLinkId,
                r.InsuranceCompanyName,
                r.Premium,
                r.CoverageAmount,
                r.MaxCoverage,
                r.IsEligible,
                r.ErrorMessage)),
            ApiResults.Problem);
    }

    private static async Task<IResult> HandleWarrantyAsync(
        Guid publicSaleId,
        ISender sender,
        CancellationToken ct)
    {
        var command = new GenerateWarrantyQuoteCommand(publicSaleId);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.Ok(new WarrantyQuoteResponse(
                r.Premium,
                r.SalesTaxPremium,
                r.WarrantySelected)),
            ApiResults.Problem);
    }

    private static async Task<IResult> HandleOutsideAsync(
        Guid publicSaleId,
        HttpContext httpContext,
        ISender sender,
        CancellationToken ct)
    {
        var request = await httpContext.Request.ReadFromJsonAsync<RecordOutsideInsuranceRequest>(ct);
        if (request is null)
            return Results.BadRequest("Request body is required for outside insurance.");

        var command = new RecordOutsideInsuranceCommand(
            publicSaleId,
            request.ProviderName,
            request.CoverageAmount,
            request.PremiumAmount);

        var result = await sender.Send(command, ct);

        return result.Match(
            () => Results.Created(),
            ApiResults.Problem);
    }
}

// ?type=home-first — request body
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

// ?type=home-first — response
public sealed record HomeFirstQuoteResponse(
    int TempLinkId,
    string InsuranceCompanyName,
    decimal Premium,
    decimal CoverageAmount,
    decimal MaxCoverage,
    bool IsEligible,
    string? ErrorMessage);

// ?type=warranty — no request body; response
public sealed record WarrantyQuoteResponse(
    decimal Premium,
    decimal SalesTaxPremium,
    bool WarrantySelected);

// ?type=outside — request body (201 Created, no response body)
public sealed record RecordOutsideInsuranceRequest(
    string ProviderName,
    decimal CoverageAmount,
    decimal PremiumAmount);

// ?type=print — request body
public sealed record PrintInsuranceQuoteRequest(
    int TempLinkId,
    decimal CoverageAmount,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Email,
    string? Address1,
    string? City,
    string? State,
    string? Zip);

// ?type=print — response
public sealed record PrintInsuranceQuoteResponse(
    string FileBase64,
    string FileName,
    string ContentType);
