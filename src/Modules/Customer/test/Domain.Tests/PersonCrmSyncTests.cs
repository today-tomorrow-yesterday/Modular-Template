using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PersonCrmSyncTests
{
    [Fact]
    public void UpdateFromCrmSync_raises_NameChanged_only_when_name_actually_changes()
    {
        var person = CreateTestPerson();
        person.ClearDomainEvents();

        // Same name — no event
        person.UpdateFromCrmSync(
            PersonName.Create("John", null, "Doe"),
            null, [], person.HomeCenterNumber, null, null, null);
        Assert.DoesNotContain(person.DomainEvents, e => e is PartyNameChangedDomainEvent);

        // Different name — event raised
        person.UpdateFromCrmSync(
            PersonName.Create("John", "Q", "Doe"),
            null, [], person.HomeCenterNumber, null, null, null);
        Assert.Single(person.DomainEvents.OfType<PartyNameChangedDomainEvent>());
    }

    [Fact]
    public void UpdateFromCrmSync_raises_SalesAssignmentsChanged_only_when_set_differs()
    {
        var assignments = new[] { ("SP-1", SalesAssignmentRole.Primary) };
        var person = Person.SyncFromCrm(42, 100, LifecycleStage.Lead,
            PersonName.Create("Jane", null, "Doe"), null, assignments, null, null, null, null);
        person.ClearDomainEvents();

        // Same assignments — no event
        person.UpdateFromCrmSync(person.Name, null, assignments, person.HomeCenterNumber, null, null, null);
        Assert.DoesNotContain(person.DomainEvents, e => e is PartySalesAssignmentsChangedDomainEvent);

        // Different assignments — event raised
        var newAssignments = new[] { ("SP-1", SalesAssignmentRole.Primary), ("SP-2", SalesAssignmentRole.Supporting) };
        person.UpdateFromCrmSync(person.Name, null, newAssignments, person.HomeCenterNumber, null, null, null);
        Assert.Single(person.DomainEvents.OfType<PartySalesAssignmentsChangedDomainEvent>());
    }

    [Fact]
    public void ApplySharedCrmSyncFields_detects_home_center_and_address_changes_independently()
    {
        var person = CreateTestPerson();
        person.ClearDomainEvents();

        var newAddress = MailingAddress.Create("123 Main", null, "Dallas", null, "TX", "US", "75201");

        // Change both home center and address
        person.UpdateFromCrmSync(
            person.Name, null, [], 999, null, newAddress, null);

        Assert.Single(person.DomainEvents.OfType<PartyHomeCenterChangedDomainEvent>());
        Assert.Single(person.DomainEvents.OfType<PartyMailingAddressChangedDomainEvent>());
        person.ClearDomainEvents();

        // Repeat same values — no events
        person.UpdateFromCrmSync(
            person.Name, null, [], 999, null, newAddress, null);

        Assert.DoesNotContain(person.DomainEvents, e => e is PartyHomeCenterChangedDomainEvent);
        Assert.DoesNotContain(person.DomainEvents, e => e is PartyMailingAddressChangedDomainEvent);
    }

    [Fact]
    public void SyncFromCrm_wires_up_sales_assignments_with_correct_roles()
    {
        var assignments = new[]
        {
            ("SP-PRIMARY", SalesAssignmentRole.Primary),
            ("SP-SUPPORT", SalesAssignmentRole.Supporting)
        };

        var person = Person.SyncFromCrm(1, 100, LifecycleStage.Opportunity,
            PersonName.Create("Test", null, "User"), null, assignments, null, null, null, null);

        Assert.Equal(2, person.SalesAssignments.Count);
        Assert.Equal("SP-PRIMARY", person.GetPrimarySalesPersonId());
        Assert.Contains(person.SalesAssignments, sa => sa.SalesPersonId == "SP-SUPPORT" && sa.Role == SalesAssignmentRole.Supporting);
    }

    private static Person CreateTestPerson() =>
        Person.SyncFromCrm(1, 100, LifecycleStage.Lead,
            PersonName.Create("John", null, "Doe"), null, [], null, null, null, null);
}
