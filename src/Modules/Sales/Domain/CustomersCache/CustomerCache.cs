using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Caching;

namespace Modules.Sales.Domain.CustomersCache;

public enum LifecycleStage
{
    Lead,
    Opportunity,
    Customer
}

public sealed class CustomerCache : ICacheProjection
{
    public int Id { get; set; }
    public Guid RefPublicId { get; set; }
    public LifecycleStage LifecycleStage { get; set; }
    public int HomeCenterNumber { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? SalesforceAccountId { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }

    [SensitiveData] public string FirstName { get; set; } = string.Empty;
    [SensitiveData] public string? MiddleName { get; set; }
    [SensitiveData] public string LastName { get; set; } = string.Empty;
    [SensitiveData] public string? Email { get; set; }
    [SensitiveData] public string? Phone { get; set; }
    [SensitiveData] public string? CoBuyerFirstName { get; set; }
    [SensitiveData] public string? CoBuyerLastName { get; set; }
    public string? PrimarySalesPersonFederatedId { get; set; }
    public string? PrimarySalesPersonFirstName { get; set; }
    public string? PrimarySalesPersonLastName { get; set; }
    public string? SecondarySalesPersonFederatedId { get; set; }
    public string? SecondarySalesPersonFirstName { get; set; }
    public string? SecondarySalesPersonLastName { get; set; }
}
