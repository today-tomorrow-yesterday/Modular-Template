using Rtl.Core.Domain;

namespace Modules.Sales.Domain.PartiesCache;

public interface IPartyCacheRepository : IReadRepository<PartyCache, int>
{
    Task<PartyCache?> GetByRefPublicIdAsync(Guid refPublicId, CancellationToken cancellationToken = default);
}
