using Rtl.Core.Domain.Caching;

namespace Modules.Sales.Domain.PartiesCache;

public enum PartyType
{
    Person,
    Organization
}

public enum LifecycleStage
{
    Lead,
    Opportunity,
    Customer
}

// ECST Cache Entity — cache.parties (TPT base).
// Populated by PartyCreated (polymorphic — Person or Organization) and PartyOnboardedFromLoan (Person only).
// PartyType discriminates detail tables: cache.party_persons or cache.party_organizations.
public sealed class PartyCache : ICacheProjection
{
    public int Id { get; set; }
    public Guid RefPublicId { get; set; } // Upsert key
    public PartyType PartyType { get; set; }
    public LifecycleStage LifecycleStage { get; set; }
    public int HomeCenterNumber { get; set; } // Used as MHC in iSeries commission request
    public string DisplayName { get; set; } = string.Empty; // Person: "FirstName LastName", Org: "OrgName"
    public string? SalesforceAccountId { get; set; }
    public DateTime LastSyncedAtUtc { get; set; }

    // TPT navigation (one-to-one, only one will be non-null based on PartyType)
    public PartyPersonCache? Person { get; set; }
    public PartyOrganizationCache? Organization { get; set; }
}
