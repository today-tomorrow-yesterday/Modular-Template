namespace Modules.Sales.Domain.AuthorizedUsersCache;

public interface IAuthorizedUserCacheWriter
{
    Task UpsertAsync(AuthorizedUserCache cache, CancellationToken cancellationToken = default);
}
