using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PartyContactPointTests
{
    [Fact]
    public void AddContactPoint_upserts_by_type_and_value()
    {
        var person = CreateTestPerson();

        person.AddContactPoint(ContactPointType.Email, "john@test.com", isPrimary: true);
        person.AddContactPoint(ContactPointType.Email, "john@test.com", isPrimary: false); // same type+value

        Assert.Single(person.ContactPoints);
        Assert.False(person.ContactPoints.First().IsPrimary); // Updated to non-primary
    }

    [Fact]
    public void Setting_primary_clears_other_primaries_of_same_type()
    {
        var person = CreateTestPerson();

        person.AddContactPoint(ContactPointType.Email, "old@test.com", isPrimary: true);
        person.AddContactPoint(ContactPointType.Email, "new@test.com", isPrimary: true);

        var oldEmail = person.ContactPoints.First(cp => cp.Value == "old@test.com");
        var newEmail = person.ContactPoints.First(cp => cp.Value == "new@test.com");

        Assert.False(oldEmail.IsPrimary);
        Assert.True(newEmail.IsPrimary);
    }

    [Fact]
    public void ReplaceContactPoints_raises_event_only_when_collection_changes()
    {
        var person = CreateTestPerson();
        var contacts = new[]
        {
            (ContactPointType.Email, "john@test.com", true),
            (ContactPointType.Phone, "555-1234", false)
        };

        person.ReplaceContactPoints(contacts);
        Assert.Single(person.DomainEvents.OfType<PartyContactPointsChangedDomainEvent>());
        person.ClearDomainEvents();

        // Same contacts again — no event
        person.ReplaceContactPoints(contacts);
        Assert.DoesNotContain(person.DomainEvents, e => e is PartyContactPointsChangedDomainEvent);
    }

    [Fact]
    public void OnboardFromLoan_only_adds_contact_points_for_non_null_values()
    {
        var withContacts = Person.OnboardFromLoan("LOAN-1", 100,
            PersonName.Create("John", null, "Doe"), null, "j@test.com", "5551234");
        Assert.Equal(2, withContacts.ContactPoints.Count);

        var withoutContacts = Person.OnboardFromLoan("LOAN-2", 100,
            PersonName.Create("Jane", null, "Doe"), null, null, null);
        Assert.Empty(withoutContacts.ContactPoints);

        var emailOnly = Person.OnboardFromLoan("LOAN-3", 100,
            PersonName.Create("Bob", null, "Smith"), null, "bob@test.com", "  "); // whitespace phone
        Assert.Single(emailOnly.ContactPoints);
        Assert.Equal(ContactPointType.Email, emailOnly.ContactPoints.First().Type);
    }

    private static Person CreateTestPerson() =>
        Person.SyncFromCrm(1, 100, LifecycleStage.Lead,
            PersonName.Create("John", null, "Doe"), null, [], null, null, null, null);
}
