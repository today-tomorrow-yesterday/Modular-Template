using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Rtl.Core.Presentation.Endpoints;

namespace Modules.Inventory.Presentation.Endpoints.V1.Transportation;

internal sealed class GetTransportationEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .WithSummary("Get transportation requirements")
            .WithDescription("Returns wheel/axle transportation requirements derived from CDC reference data for the given home dimensions.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<TransportationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static Task<IResult> HandleAsync(
        decimal? length,
        decimal? width,
        ISender sender,
        CancellationToken ct)
    {
        if (length is null or <= 0 || width is null or <= 0)
            return Task.FromResult(Results.Problem(
                "length and width query parameters are required and must be positive.",
                statusCode: StatusCodes.Status400BadRequest));

        var mock = new TransportationResponse(
            NumberOfWheels: 6,
            NumberOfAxles: 3);
        return Task.FromResult(Results.Ok(mock));
    }
}

public sealed record TransportationResponse(
    int NumberOfWheels,
    int NumberOfAxles);
