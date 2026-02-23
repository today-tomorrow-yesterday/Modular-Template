using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Modules.Sales.Application.DeliveryAddresses.GetDeliveryAddress;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Sales.Presentation.Endpoints.V1.DeliveryAddress;

internal sealed class GetDeliveryAddressEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/{publicSaleId:guid}/delivery-address", HandleAsync)
            .WithSummary("Get delivery address for a sale")
            .WithDescription("Returns the delivery address for a sale.")
            .WithName("GetDeliveryAddress")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<DeliveryAddressResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        Guid publicSaleId,
        ISender sender,
        CancellationToken ct)
    {
        var query = new GetDeliveryAddressQuery(publicSaleId);

        var result = await sender.Send(query, ct);

        return result.Match(
            r => Results.Ok(new DeliveryAddressResponse(
                r.Id,
                r.SaleId,
                r.OccupancyType,
                r.IsWithinCityLimits,
                r.AddressLine1,
                r.City,
                r.County,
                r.State,
                r.PostalCode)),
            ApiResults.Problem);
    }
}

public sealed record DeliveryAddressResponse(
    int Id,
    int SaleId,
    string? OccupancyType,
    bool IsWithinCityLimits,
    string? AddressLine1,
    string? City,
    string? County,
    string? State,
    string? PostalCode);
