using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.Customers.Events;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerCrmSyncTests
{
    [Fact]
    public void UpdateFromCrmSync_raises_NameChanged_only_when_name_actually_changes()
    {
        var customer = CreateTestCustomer();
        customer.ClearDomainEvents();

        // Same name — no event
        customer.UpdateFromCrmSync(
            CustomerName.Create("John", null, "Doe"),
            null, [], customer.HomeCenterNumber, null, null, null);
        Assert.DoesNotContain(customer.DomainEvents, e => e is CustomerNameChangedDomainEvent);

        // Different name — event raised
        customer.UpdateFromCrmSync(
            CustomerName.Create("John", "Q", "Doe"),
            null, [], customer.HomeCenterNumber, null, null, null);
        Assert.Single(customer.DomainEvents.OfType<CustomerNameChangedDomainEvent>());
    }

    [Fact]
    public void UpdateFromCrmSync_raises_SalesAssignmentsChanged_only_when_set_differs()
    {
        var assignments = new[] { ("SP-1", SalesAssignmentRole.Primary) };
        var customer = Customers.Entities.Customer.SyncFromCrm(42, 100, LifecycleStage.Lead,
            CustomerName.Create("Jane", null, "Doe"), null, assignments, null, null, null, null);
        customer.ClearDomainEvents();

        // Same assignments — no event
        customer.UpdateFromCrmSync(customer.Name, null, assignments, customer.HomeCenterNumber, null, null, null);
        Assert.DoesNotContain(customer.DomainEvents, e => e is CustomerSalesAssignmentsChangedDomainEvent);

        // Different assignments — event raised
        var newAssignments = new[] { ("SP-1", SalesAssignmentRole.Primary), ("SP-2", SalesAssignmentRole.Supporting) };
        customer.UpdateFromCrmSync(customer.Name, null, newAssignments, customer.HomeCenterNumber, null, null, null);
        Assert.Single(customer.DomainEvents.OfType<CustomerSalesAssignmentsChangedDomainEvent>());
    }

    [Fact]
    public void ApplySharedCrmSyncFields_detects_home_center_and_address_changes_independently()
    {
        var customer = CreateTestCustomer();
        customer.ClearDomainEvents();

        var newAddress = MailingAddress.Create("123 Main", null, "Dallas", null, "TX", "US", "75201");

        // Change both home center and address
        customer.UpdateFromCrmSync(
            customer.Name, null, [], 999, null, newAddress, null);

        Assert.Single(customer.DomainEvents.OfType<CustomerHomeCenterChangedDomainEvent>());
        Assert.Single(customer.DomainEvents.OfType<CustomerMailingAddressChangedDomainEvent>());
        customer.ClearDomainEvents();

        // Repeat same values — no events
        customer.UpdateFromCrmSync(
            customer.Name, null, [], 999, null, newAddress, null);

        Assert.DoesNotContain(customer.DomainEvents, e => e is CustomerHomeCenterChangedDomainEvent);
        Assert.DoesNotContain(customer.DomainEvents, e => e is CustomerMailingAddressChangedDomainEvent);
    }

    [Fact]
    public void SyncFromCrm_wires_up_sales_assignments_with_correct_roles()
    {
        var assignments = new[]
        {
            ("SP-PRIMARY", SalesAssignmentRole.Primary),
            ("SP-SUPPORT", SalesAssignmentRole.Supporting)
        };

        var customer = Customers.Entities.Customer.SyncFromCrm(1, 100, LifecycleStage.Opportunity,
            CustomerName.Create("Test", null, "User"), null, assignments, null, null, null, null);

        Assert.Equal(2, customer.SalesAssignments.Count);
        Assert.Equal("SP-PRIMARY", customer.GetPrimarySalesPersonId());
        Assert.Contains(customer.SalesAssignments, sa => sa.SalesPersonId == "SP-SUPPORT" && sa.Role == SalesAssignmentRole.Supporting);
    }

    private static Customers.Entities.Customer CreateTestCustomer() =>
        Customers.Entities.Customer.SyncFromCrm(1, 100, LifecycleStage.Lead,
            CustomerName.Create("John", null, "Doe"), null, [], null, null, null, null);
}
