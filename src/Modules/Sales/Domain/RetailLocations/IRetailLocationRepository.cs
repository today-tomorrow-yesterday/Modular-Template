using Rtl.Core.Domain;

namespace Modules.Sales.Domain.RetailLocations;

public interface IRetailLocationRepository : IRepository<RetailLocation, int>
{
    Task<RetailLocation?> GetByHomeCenterNumberAsync(int homeCenterNumber, CancellationToken cancellationToken = default);
}
