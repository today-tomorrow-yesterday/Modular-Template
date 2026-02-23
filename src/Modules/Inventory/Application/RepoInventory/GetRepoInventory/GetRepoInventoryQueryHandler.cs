using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Results;

namespace Modules.Inventory.Application.RepoInventory.GetRepoInventory;

// Flow: GET /api/v1/inventory/repo → sync HTTP to iSeries (dynamic geo params)
internal sealed class GetRepoInventoryQueryHandler
    : IQueryHandler<GetRepoInventoryQuery, IReadOnlyCollection<RepoInventoryResponse>>
{
    public Task<Result<IReadOnlyCollection<RepoInventoryResponse>>> Handle(
        GetRepoInventoryQuery request,
        CancellationToken cancellationToken)
    {
        // TODO: Implement iSeries HTTP call when infrastructure is available.
        // 1. Look up home center coordinates from cache.home_centers (populated via Organization ECST)
        // 2. Call iSeries HTTP with lat/long/maxDistance (default 200 miles)
        // 3. Calculate Haversine distance to filter within radius
        // 4. Return RepoHome[]
        return Task.FromResult(
            Result.Success<IReadOnlyCollection<RepoInventoryResponse>>(
                Array.Empty<RepoInventoryResponse>()));
    }
}
