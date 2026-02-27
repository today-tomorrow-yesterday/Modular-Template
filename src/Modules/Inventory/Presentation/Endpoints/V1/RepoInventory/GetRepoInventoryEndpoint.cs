using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Rtl.Core.Domain.Results;
using Rtl.Core.Presentation.Endpoints;
using Rtl.Core.Presentation.Results;

namespace Modules.Inventory.Presentation.Endpoints.V1.RepoInventory;

internal sealed class GetRepoInventoryEndpoint : IEndpoint
{
    public void MapEndpoint(RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .WithSummary("Get repo inventory")
            .WithDescription("Returns repossessed homes by geographic location or account. Use latitude/longitude/maxDistance for geo search, or accountId for account-based search.")
            .MapToApiVersion(new ApiVersion(1, 0))
            .Produces<ApiEnvelope<IReadOnlyCollection<RepoInventoryResponse>>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static Task<IResult> HandleAsync(
        decimal? latitude,
        decimal? longitude,
        decimal? maxDistance,
        string? accountId,
        ISender sender,
        CancellationToken ct)
    {
        bool hasGeo = latitude.HasValue && longitude.HasValue && maxDistance.HasValue;
        bool hasAccount = !string.IsNullOrWhiteSpace(accountId);

        if (!hasGeo && !hasAccount)
            return Task.FromResult(ApiResponse.Problem(
                Error.Validation("Search.Invalid", "Provide either latitude/longitude/maxDistance or accountId.")));

        var mock = new[]
        {
            new RepoInventoryResponse(
                StockNumber: "RPO-001",
                Make: "Clayton",
                Model: "Pegasus",
                ModelYear: 2024,
                SalePrice: 45000.00m)
        };
        return Task.FromResult(ApiResponse.Ok<IReadOnlyCollection<RepoInventoryResponse>>(mock));
    }
}

public sealed record RepoInventoryResponse(
    string StockNumber,
    string Make,
    string Model,
    int ModelYear,
    decimal SalePrice);
