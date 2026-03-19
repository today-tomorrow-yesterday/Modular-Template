using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerLifecycleTests
{
    [Fact]
    public void Lead_to_Opportunity_to_Customer_is_the_only_valid_progression()
    {
        var customer = Customers.Entities.Customer.CreateLead(100, CustomerName.Create("John", null, "Doe"), "SF-LEAD-1", null);
        customer.ClearDomainEvents();

        var toOpportunity = customer.PromoteToOpportunity("SF-OPP-1");
        Assert.True(toOpportunity.IsSuccess);
        Assert.Equal(LifecycleStage.Opportunity, customer.LifecycleStage);
        Assert.Single(customer.DomainEvents.OfType<CustomerLifecycleAdvancedDomainEvent>());

        customer.ClearDomainEvents();
        var toCustomer = customer.PromoteToCustomer("SF-ACCT-1");
        Assert.True(toCustomer.IsSuccess);
        Assert.Equal(LifecycleStage.Customer, customer.LifecycleStage);

        // Identifiers added for each transition
        Assert.Equal("SF-OPP-1", customer.GetIdentifierValue(IdentifierType.SalesforceOpportunityId));
        Assert.Equal("SF-ACCT-1", customer.GetIdentifierValue(IdentifierType.SalesforceAccountId));
    }

    [Fact]
    public void Cannot_skip_stages_or_go_backwards()
    {
        var lead = Customers.Entities.Customer.CreateLead(100, CustomerName.Create("Jane", null, "Doe"), "SF-LEAD-2", null);

        // Lead cannot jump to Customer
        Assert.True(lead.PromoteToCustomer("SF-ACCT-X").IsFailure);
        Assert.Equal(LifecycleStage.Lead, lead.LifecycleStage);

        lead.PromoteToOpportunity("SF-OPP-2");

        // Opportunity cannot go back to Opportunity or Lead
        Assert.True(lead.PromoteToOpportunity("SF-OPP-AGAIN").IsFailure);
        Assert.Equal(LifecycleStage.Opportunity, lead.LifecycleStage);

        lead.PromoteToCustomer("SF-ACCT-2");

        // Customer cannot promote further
        Assert.True(lead.PromoteToCustomer("SF-ACCT-AGAIN").IsFailure);
        Assert.True(lead.PromoteToOpportunity("SF-OPP-AGAIN").IsFailure);
    }

    [Fact]
    public void OnboardFromLoan_creates_directly_as_Customer_stage()
    {
        var customer = Customers.Entities.Customer.OnboardFromLoan("LOAN-1", 200, CustomerName.Create("Bob", null, "Smith"), null, null, null);

        Assert.Equal(LifecycleStage.Customer, customer.LifecycleStage);
        Assert.Equal("LOAN-1", customer.GetIdentifierValue(IdentifierType.LoanId));
        Assert.Single(customer.DomainEvents.OfType<CustomerOnboardedFromLoanDomainEvent>());
    }
}
