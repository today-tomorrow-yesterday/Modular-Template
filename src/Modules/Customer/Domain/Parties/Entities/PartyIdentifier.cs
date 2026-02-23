using Modules.Customer.Domain.Parties.Enums;
using Rtl.Core.Domain.Entities;

namespace Modules.Customer.Domain.Parties.Entities;

// External system identifier for a Party. One per type (unique constraint on party_id + type).
// Enterprise uses untyped key/value for flexibility; typed enum is more appropriate for our
// bounded context — known, finite set of source systems.
public sealed class PartyIdentifier : Entity
{
    private PartyIdentifier() {}

    public int PartyId { get; private set; }
    public IdentifierType Type { get; private set; }
    public string Value { get; private set; } = null!;

    internal static PartyIdentifier Create(int partyId, IdentifierType type, string value)
    {
        return new PartyIdentifier
        {
            PartyId = partyId,
            Type = type,
            Value = value
        };
    }

    internal void UpdateValue(string value)
    {
        Value = value;
    }
}
