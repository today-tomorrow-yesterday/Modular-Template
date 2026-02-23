using Rtl.Core.Domain;

namespace Modules.Organization.Domain.ManualAssignments;

public interface IManualHomeCenterAssignmentRepository : IReadRepository<ManualHomeCenterAssignment, int>
{
    Task<IReadOnlyCollection<ManualHomeCenterAssignment>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
