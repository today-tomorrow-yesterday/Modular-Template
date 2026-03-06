using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.DeliveryAddresses.CreateDeliveryAddress;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;

internal sealed class CreateDeliveryAddressEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapPost("/{publicSaleId:guid}/delivery-address", HandleAsync)
            .WithSummary("Create delivery address for a sale")
            .WithDescription("Creates the delivery address. Only one delivery address per sale.")
            .WithName("CreateDeliveryAddress")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<CreateDeliveryAddressResponse>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithMetadata(new RequestBodyExample(Examples.Request));
    }

    internal static class Examples
    {
        public const string Request = """
        {
            "occupancyType": "Primary Residence",
            "isWithinCityLimits": true,
            "addressLine1": "5000 Clayton Rd",
            "city": "Maryville",
            "county": "Blount",
            "state": "TN",
            "postalCode": "37801"
        }
        """;
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        CreateDeliveryAddressRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var command = new CreateDeliveryAddressCommand(
            publicSaleId,
            request.OccupancyType,
            request.IsWithinCityLimits,
            request.AddressLine1,
            request.City,
            request.County,
            request.State,
            request.PostalCode);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => ApiResponse.Created($"/api/v1/sales/{publicSaleId}/delivery-address", new CreateDeliveryAddressResponse(r.PublicId)),
            ApiResponse.Problem);
    }
}

public sealed record CreateDeliveryAddressRequest(
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode);

public sealed record CreateDeliveryAddressResponse(Guid Id);
