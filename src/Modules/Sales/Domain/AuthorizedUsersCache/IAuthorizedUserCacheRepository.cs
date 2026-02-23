using Rtl.Core.Domain;

namespace Modules.Sales.Domain.AuthorizedUsersCache;

public interface IAuthorizedUserCacheRepository : IReadRepository<AuthorizedUserCache, int>
{
    Task<AuthorizedUserCache?> GetByFederatedIdAsync(string federatedId, CancellationToken cancellationToken = default);

    Task<AuthorizedUserCache?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
}
