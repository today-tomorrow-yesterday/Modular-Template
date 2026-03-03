using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Funding.Domain.CustomersCache;

/// <summary>
/// Minimal customer cache for funding request validation and LoanId correlation.
/// </summary>
public sealed class CustomerCache : ICacheProjection
{
    public int Id { get; set; }
    public Guid RefPublicId { get; set; }
    [SensitiveData] public string? LoanId { get; set; }
    [SensitiveData] public string FirstName { get; set; } = string.Empty;
    [SensitiveData] public string LastName { get; set; } = string.Empty;
    public int HomeCenterNumber { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }
}
