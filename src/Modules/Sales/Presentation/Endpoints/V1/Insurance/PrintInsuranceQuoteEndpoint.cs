using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.Insurance;

internal sealed class PrintInsuranceQuoteEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/insurance/quote/print", HandleAsync)
            .WithSummary("Print HomeFirst quote sheet PDF")
            .WithDescription("Calls iSeries adapter to generate a printable HomeFirst insurance quote PDF.")
            .WithName("PrintInsuranceQuote")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<PrintInsuranceQuoteResponse>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "tempLinkId": 12345,
            "coverageAmount": 300000.00,
            "firstName": "John",
            "lastName": "Doe",
            "phone": "8655551234",
            "email": "john.doe@example.com",
            "address1": "5000 Clayton Rd",
            "city": "Maryville",
            "state": "TN",
            "zip": "37801"
        }
        """;
    }

    private static Task<IResult> HandleAsync(
        Guid publicSaleId,
        PrintInsuranceQuoteRequest request,
        CancellationToken ct)
    {
        // Stub — returns a placeholder PDF response until iSeries print adapter is wired.
        return Task.FromResult(ApiResponse.Ok(new PrintInsuranceQuoteResponse(
            FileBase64: "JVBERi0xLjQg...",
            FileName: $"HomeFirst_Quote_{request.TempLinkId}.pdf",
            ContentType: "application/pdf")));
    }
}

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

public sealed record PrintInsuranceQuoteResponse(
    string FileBase64,
    string FileName,
    string ContentType);
