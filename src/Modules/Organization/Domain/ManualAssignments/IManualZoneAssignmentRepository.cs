using Rtl.Core.Domain;

namespace Modules.Organization.Domain.ManualAssignments;

public interface IManualZoneAssignmentRepository : IReadRepository<ManualZoneAssignment, int>
{
    Task<IReadOnlyCollection<ManualZoneAssignment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
