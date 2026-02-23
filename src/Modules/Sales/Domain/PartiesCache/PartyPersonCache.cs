using Rtl.Core.Domain.Auditing;

namespace Modules.Sales.Domain.PartiesCache;

// ECST Cache Entity — cache.party_persons. TPT detail for Person parties.
// Critical for commission calculation — PrimarySalesPersonFederatedId resolves EmployeeNumber via cache.authorized_users.
public sealed class PartyPersonCache
{
    public int PartyId { get; set; }
    [SensitiveData] public string FirstName { get; set; } = string.Empty; // Used in SaleSummaryChanged event
    [SensitiveData] public string? MiddleName { get; set; }
    [SensitiveData] public string LastName { get; set; } = string.Empty; // Used in SaleSummaryChanged event
    [SensitiveData] public string? Email { get; set; } // Extracted from ContactPoints[Type=="Email"]
    [SensitiveData] public string? Phone { get; set; } // Extracted from ContactPoints[Type=="Phone"]
    [SensitiveData] public string? CoBuyerFirstName { get; set; } // Denormalized from CoBuyer Party
    [SensitiveData] public string? CoBuyerLastName { get; set; } // Denormalized from CoBuyer Party
    public string? PrimarySalesPersonFederatedId { get; set; } // Resolves EmployeeNumber for commission
    public string? PrimarySalesPersonFirstName { get; set; }
    public string? PrimarySalesPersonLastName { get; set; }
    public string? SecondarySalesPersonFederatedId { get; set; } // Resolves EmployeeNumber (if present)
    public string? SecondarySalesPersonFirstName { get; set; }
    public string? SecondarySalesPersonLastName { get; set; }

    // TPT navigation
    public PartyCache Party { get; set; } = null!;
}
