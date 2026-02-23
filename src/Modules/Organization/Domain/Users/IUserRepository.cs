using Rtl.Core.Domain;

namespace Modules.Organization.Domain.Users;

public interface IUserRepository : IReadRepository<User, int>
{
    Task<User?> GetByFederatedIdAsync(string federatedId, CancellationToken cancellationToken = default);

    Task<User?> GetByEmployeeNumberAsync(int employeeNumber, CancellationToken cancellationToken = default);
}
