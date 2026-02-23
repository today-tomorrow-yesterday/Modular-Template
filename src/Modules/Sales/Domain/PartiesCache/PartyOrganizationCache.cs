namespace Modules.Sales.Domain.PartiesCache;

// ECST Cache Entity — cache.party_organizations. TPT detail for Organization parties.
public sealed class PartyOrganizationCache
{
    public int PartyId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;

    // TPT navigation
    public PartyCache Party { get; set; } = null!;
}
