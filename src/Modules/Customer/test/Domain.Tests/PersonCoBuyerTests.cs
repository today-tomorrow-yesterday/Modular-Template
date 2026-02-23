using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.Parties.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PersonCoBuyerTests
{
    [Fact]
    public void SetCoBuyer_is_idempotent_and_RemoveCoBuyer_clears_the_link()
    {
        var person = CreateTestPerson();

        person.SetCoBuyer(42);
        Assert.Equal(42, person.CoBuyerPartyId);
        Assert.Single(person.DomainEvents.OfType<PartyCoBuyerChangedDomainEvent>());

        person.ClearDomainEvents();
        person.SetCoBuyer(42); // Same value — no event
        Assert.Empty(person.DomainEvents.OfType<PartyCoBuyerChangedDomainEvent>());

        person.RemoveCoBuyer();
        Assert.Null(person.CoBuyerPartyId);
        Assert.Single(person.DomainEvents.OfType<PartyCoBuyerChangedDomainEvent>());

        person.ClearDomainEvents();
        person.RemoveCoBuyer(); // Already null — no event
        Assert.Empty(person.DomainEvents.OfType<PartyCoBuyerChangedDomainEvent>());
    }

    private static Person CreateTestPerson() =>
        Person.SyncFromCrm(1, 100, LifecycleStage.Lead,
            PersonName.Create("Test", null, "User"), null, [], null, null, null, null);
}
