using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class OrganizationTests
{
    [Fact]
    public void CreateLead_sets_correct_state_and_raises_PartyCreated()
    {
        var org = Organization.CreateLead(100, "Acme Homes", "SF-LEAD-1", "https://sf.example.com");

        Assert.Equal(PartyType.Organization, org.PartyType);
        Assert.Equal(LifecycleStage.Lead, org.LifecycleStage);
        Assert.Equal("Acme Homes", org.OrganizationName);
        Assert.Equal("SF-LEAD-1", org.GetIdentifierValue(IdentifierType.SalesforceLeadId));
        Assert.Single(org.DomainEvents.OfType<PartyCreatedDomainEvent>());
    }

    [Fact]
    public void UpdateFromCrmSync_raises_NameChanged_only_when_name_differs()
    {
        var org = Organization.SyncFromCrm(1, 100, LifecycleStage.Lead, "Acme Homes", null, null, null, null);
        org.ClearDomainEvents();

        org.UpdateFromCrmSync("Acme Homes", 100, null, null, null);
        Assert.DoesNotContain(org.DomainEvents, e => e is PartyNameChangedDomainEvent);

        org.UpdateFromCrmSync("Acme Development Group", 100, null, null, null);
        Assert.Single(org.DomainEvents.OfType<PartyNameChangedDomainEvent>());
    }

    [Fact]
    public void Organization_follows_same_lifecycle_state_machine_as_Person()
    {
        var org = Organization.CreateLead(100, "Test Corp", "SF-LEAD-1", null);

        Assert.True(org.PromoteToCustomer("acct-1").IsFailure); // Can't skip
        Assert.True(org.PromoteToOpportunity("opp-1").IsSuccess);
        Assert.True(org.PromoteToOpportunity("opp-2").IsFailure); // Can't repeat
        Assert.True(org.PromoteToCustomer("acct-1").IsSuccess);
    }
}
