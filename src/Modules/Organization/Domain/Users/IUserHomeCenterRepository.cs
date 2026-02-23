using Rtl.Core.Domain;

namespace Modules.Organization.Domain.Users;

public interface IUserHomeCenterRepository : IReadRepository<UserHomeCenter, int>
{
    Task<IReadOnlyCollection<UserHomeCenter>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<UserHomeCenter>> GetByHomeCenterIdAsync(int homeCenterId, CancellationToken cancellationToken = default);
}
