using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerIdentifierTests
{
    [Fact]
    public void AddIdentifier_upserts_by_type_so_repeated_syncs_dont_create_duplicates()
    {
        var customer = Customers.Entities.Customer.CreateLead(100, CustomerName.Create("John", null, "Doe"), "SF-LEAD-1", null);

        // SalesforceLeadId was set by CreateLead. Overwrite it.
        customer.AddIdentifier(IdentifierType.SalesforceLeadId, "SF-LEAD-UPDATED");

        Assert.Single(customer.Identifiers, i => i.Type == IdentifierType.SalesforceLeadId);
        Assert.Equal("SF-LEAD-UPDATED", customer.GetIdentifierValue(IdentifierType.SalesforceLeadId));

        // Add a different type
        customer.AddIdentifier(IdentifierType.LoanId, "LOAN-999");

        Assert.Equal(2, customer.Identifiers.Count);
        Assert.Equal("LOAN-999", customer.GetIdentifierValue(IdentifierType.LoanId));
    }
}
