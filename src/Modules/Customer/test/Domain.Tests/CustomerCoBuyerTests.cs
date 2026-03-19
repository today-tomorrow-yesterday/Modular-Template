using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerCoBuyerTests
{
    [Fact]
    public void SetCoBuyer_is_idempotent_and_RemoveCoBuyer_clears_the_link()
    {
        var customer = CreateTestCustomer();

        customer.SetCoBuyer(42);
        Assert.Equal(42, customer.CoBuyerCustomerId);
        Assert.Single(customer.DomainEvents.OfType<CustomerCoBuyerChangedDomainEvent>());

        customer.ClearDomainEvents();
        customer.SetCoBuyer(42); // Same value — no event
        Assert.Empty(customer.DomainEvents.OfType<CustomerCoBuyerChangedDomainEvent>());

        customer.RemoveCoBuyer();
        Assert.Null(customer.CoBuyerCustomerId);
        Assert.Single(customer.DomainEvents.OfType<CustomerCoBuyerChangedDomainEvent>());

        customer.ClearDomainEvents();
        customer.RemoveCoBuyer(); // Already null — no event
        Assert.Empty(customer.DomainEvents.OfType<CustomerCoBuyerChangedDomainEvent>());
    }

    private static Customers.Entities.Customer CreateTestCustomer() =>
        Customers.Entities.Customer.SyncFromCrm(1, 100, LifecycleStage.Lead,
            CustomerName.Create("Test", null, "User"), null, [], null, null, null, null);
}
