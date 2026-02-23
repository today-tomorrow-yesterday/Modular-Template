using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PersonLifecycleTests
{
    [Fact]
    public void Lead_to_Opportunity_to_Customer_is_the_only_valid_progression()
    {
        var person = Person.CreateLead(100, PersonName.Create("John", null, "Doe"), "SF-LEAD-1", null);
        person.ClearDomainEvents();

        var toOpportunity = person.PromoteToOpportunity("SF-OPP-1");
        Assert.True(toOpportunity.IsSuccess);
        Assert.Equal(LifecycleStage.Opportunity, person.LifecycleStage);
        Assert.Single(person.DomainEvents.OfType<PartyLifecycleAdvancedDomainEvent>());

        person.ClearDomainEvents();
        var toCustomer = person.PromoteToCustomer("SF-ACCT-1");
        Assert.True(toCustomer.IsSuccess);
        Assert.Equal(LifecycleStage.Customer, person.LifecycleStage);

        // Identifiers added for each transition
        Assert.Equal("SF-OPP-1", person.GetIdentifierValue(IdentifierType.SalesforceOpportunityId));
        Assert.Equal("SF-ACCT-1", person.GetIdentifierValue(IdentifierType.SalesforceAccountId));
    }

    [Fact]
    public void Cannot_skip_stages_or_go_backwards()
    {
        var lead = Person.CreateLead(100, PersonName.Create("Jane", null, "Doe"), "SF-LEAD-2", null);

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
        var person = Person.OnboardFromLoan("LOAN-1", 200, PersonName.Create("Bob", null, "Smith"), null, null, null);

        Assert.Equal(LifecycleStage.Customer, person.LifecycleStage);
        Assert.Equal("LOAN-1", person.GetIdentifierValue(IdentifierType.LoanId));
        Assert.Single(person.DomainEvents.OfType<PartyOnboardedFromLoanDomainEvent>());
    }
}
