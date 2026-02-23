namespace Modules.Inventory.Domain.HomeCentersCache;

public interface IHomeCenterCacheWriter
{
    Task UpsertAsync(HomeCenterCache cache, CancellationToken cancellationToken = default);
}
