using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PartyIdentifierTests
{
    [Fact]
    public void AddIdentifier_upserts_by_type_so_repeated_syncs_dont_create_duplicates()
    {
        var person = Person.CreateLead(100, PersonName.Create("John", null, "Doe"), "SF-LEAD-1", null);

        // SalesforceLeadId was set by CreateLead. Overwrite it.
        person.AddIdentifier(IdentifierType.SalesforceLeadId, "SF-LEAD-UPDATED");

        Assert.Single(person.Identifiers, i => i.Type == IdentifierType.SalesforceLeadId);
        Assert.Equal("SF-LEAD-UPDATED", person.GetIdentifierValue(IdentifierType.SalesforceLeadId));

        // Add a different type
        person.AddIdentifier(IdentifierType.LoanId, "LOAN-999");

        Assert.Equal(2, person.Identifiers.Count);
        Assert.Equal("LOAN-999", person.GetIdentifierValue(IdentifierType.LoanId));
    }
}
