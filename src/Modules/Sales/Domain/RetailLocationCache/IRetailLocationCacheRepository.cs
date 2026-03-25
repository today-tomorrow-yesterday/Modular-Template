using Rtl.Core.Domain;

namespace Modules.Sales.Domain.RetailLocationCache;

public interface IRetailLocationCacheRepository : IReadRepository<RetailLocationCache, int>
{
    void Add(RetailLocationCache entity);
    Task<RetailLocationCache?> GetByHomeCenterNumberAsync(int homeCenterNumber, CancellationToken cancellationToken = default);
}
