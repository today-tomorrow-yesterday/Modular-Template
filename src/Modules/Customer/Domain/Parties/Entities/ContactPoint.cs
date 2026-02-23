using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Customer.Domain.Parties.Entities;

// Contact method for a Party. Simplified Type + Value model (sufficient for retail bounded context).
//
// Enterprise Contact Point 1.0 is polymorphic with structured sub-types and Purpose 1.0
// (decouples "what" from "why" with rank ordering). Neither needed for SES Pro retail.
public sealed class ContactPoint : Entity
{
    private ContactPoint() {}

    public int PartyId { get; private set; }
    public ContactPointType Type { get; private set; }
    [SensitiveData] public string Value { get; private set; } = null!;
    public bool IsPrimary { get; private set; }

    internal static ContactPoint Create(int partyId, ContactPointType type, string value, bool isPrimary = false)
    {
        return new ContactPoint
        {
            PartyId = partyId,
            Type = type,
            Value = value,
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    internal void UpdateValue(string value) => Value = value;
}
