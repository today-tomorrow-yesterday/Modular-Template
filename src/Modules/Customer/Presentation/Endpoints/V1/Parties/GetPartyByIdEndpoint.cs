using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Customer.Application.Parties.GetPartyByPublicId;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Customer.Presentation.Endpoints.V1.Parties;

internal sealed class GetPartyByIdEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetPartyByIdAsync)
            .WithSummary("Get a party by public ID")
            .WithDescription("Retrieves a party by their unique public identifier (GUID).")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<PartyResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetPartyByIdAsync(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetPartyByPublicIdQuery(id);

        var result = await sender.Send(query, cancellationToken);

        return result.Match(Results.Ok, ApiResults.Problem);
    }
}
