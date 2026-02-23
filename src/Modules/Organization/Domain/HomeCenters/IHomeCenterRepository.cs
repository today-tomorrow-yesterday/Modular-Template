using Rtl.Core.Domain;

namespace Modules.Organization.Domain.HomeCenters;

public interface IHomeCenterRepository : IReadRepository<HomeCenter, int>
{
    Task<HomeCenter?> GetByHomeCenterNumberAsync(int homeCenterNumber, CancellationToken cancellationToken = default);
}
