using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerContactPointTests
{
    [Fact]
    public void AddContactPoint_upserts_by_type_and_value()
    {
        var customer = CreateTestCustomer();

        customer.AddContactPoint(ContactPointType.Email, "john@test.com", isPrimary: true);
        customer.AddContactPoint(ContactPointType.Email, "john@test.com", isPrimary: false); // same type+value

        Assert.Single(customer.ContactPoints);
        Assert.False(customer.ContactPoints.First().IsPrimary); // Updated to non-primary
    }

    [Fact]
    public void Setting_primary_clears_other_primaries_of_same_type()
    {
        var customer = CreateTestCustomer();

        customer.AddContactPoint(ContactPointType.Email, "old@test.com", isPrimary: true);
        customer.AddContactPoint(ContactPointType.Email, "new@test.com", isPrimary: true);

        var oldEmail = customer.ContactPoints.First(cp => cp.Value == "old@test.com");
        var newEmail = customer.ContactPoints.First(cp => cp.Value == "new@test.com");

        Assert.False(oldEmail.IsPrimary);
        Assert.True(newEmail.IsPrimary);
    }

    [Fact]
    public void ReplaceContactPoints_raises_event_only_when_collection_changes()
    {
        var customer = CreateTestCustomer();
        var contacts = new[]
        {
            (ContactPointType.Email, "john@test.com", true),
            (ContactPointType.Phone, "555-1234", false)
        };

        customer.ReplaceContactPoints(contacts);
        Assert.Single(customer.DomainEvents.OfType<CustomerContactPointsChangedDomainEvent>());
        customer.ClearDomainEvents();

        // Same contacts again — no event
        customer.ReplaceContactPoints(contacts);
        Assert.DoesNotContain(customer.DomainEvents, e => e is CustomerContactPointsChangedDomainEvent);
    }

    [Fact]
    public void OnboardFromLoan_only_adds_contact_points_for_non_null_values()
    {
        var withContacts = Customers.Entities.Customer.OnboardFromLoan("LOAN-1", 100,
            CustomerName.Create("John", null, "Doe"), null, "j@test.com", "5551234");
        Assert.Equal(2, withContacts.ContactPoints.Count);

        var withoutContacts = Customers.Entities.Customer.OnboardFromLoan("LOAN-2", 100,
            CustomerName.Create("Jane", null, "Doe"), null, null, null);
        Assert.Empty(withoutContacts.ContactPoints);

        var emailOnly = Customers.Entities.Customer.OnboardFromLoan("LOAN-3", 100,
            CustomerName.Create("Bob", null, "Smith"), null, "bob@test.com", "  "); // whitespace phone
        Assert.Single(emailOnly.ContactPoints);
        Assert.Equal(ContactPointType.Email, emailOnly.ContactPoints.First().Type);
    }

    private static Customers.Entities.Customer CreateTestCustomer() =>
        Customers.Entities.Customer.SyncFromCrm(1, 100, LifecycleStage.Lead,
            CustomerName.Create("John", null, "Doe"), null, [], null, null, null, null);
}
