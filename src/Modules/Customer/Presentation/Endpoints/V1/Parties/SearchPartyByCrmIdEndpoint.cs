using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Customer.Presentation.Endpoints.V1.Parties;

internal sealed class SearchPartyByCrmIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/search", HandleAsync)
            .WithSummary("Search party by CRM ID")
            .WithDescription("Searches for a party by their CRM identifier. Migration-period endpoint — in target ECST architecture, Sales reads from local cache.parties TPT tables instead.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<SearchPartyResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static Task<IResult> HandleAsync(
        string crmId,
        ISender sender,
        CancellationToken ct)
    {
        var mock = new SearchPartyResponse(
            Id: Guid.Parse("01924f5c-1234-7def-8abc-1234567890ab"),
            CrmId: crmId,
            FirstName: "John",
            LastName: "Doe");
        return Task.FromResult(ApiResponse.Ok(mock));
    }
}

public sealed record SearchPartyResponse(
    Guid Id,
    string CrmId,
    string FirstName,
    string LastName);
