using Modules.Customer.Domain.Customers.Entities;
using Modules.Customer.Domain.Customers.Enums;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class CustomerSalesAssignmentTests
{
    [Fact]
    public void Assigning_Primary_replaces_the_existing_Primary()
    {
        var customer = CreateTestCustomer();

        customer.AssignSalesPerson("SP-1", SalesAssignmentRole.Primary);
        customer.AssignSalesPerson("SP-2", SalesAssignmentRole.Primary);

        Assert.Equal("SP-2", customer.GetPrimarySalesPersonId());
        Assert.DoesNotContain(customer.SalesAssignments, sa => sa.SalesPersonId == "SP-1");
    }

    [Fact]
    public void Assigning_same_SalesPerson_twice_is_idempotent()
    {
        var customer = CreateTestCustomer();

        customer.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);
        customer.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);

        Assert.Single(customer.SalesAssignments);
    }

    [Fact]
    public void Multiple_Supporting_assignments_are_allowed()
    {
        var customer = CreateTestCustomer();

        customer.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);
        customer.AssignSalesPerson("SP-2", SalesAssignmentRole.Supporting);
        customer.AssignSalesPerson("SP-3", SalesAssignmentRole.Primary);

        Assert.Equal(3, customer.SalesAssignments.Count);
        Assert.Equal("SP-3", customer.GetPrimarySalesPersonId());
    }

    [Fact]
    public void RemoveSalesAssignment_removes_by_SalesPersonId()
    {
        var customer = CreateTestCustomer();
        customer.AssignSalesPerson("SP-1", SalesAssignmentRole.Primary);
        customer.AssignSalesPerson("SP-2", SalesAssignmentRole.Supporting);

        customer.RemoveSalesAssignment("SP-1");

        Assert.Single(customer.SalesAssignments);
        Assert.Null(customer.GetPrimarySalesPersonId());
    }

    private static Customers.Entities.Customer CreateTestCustomer() =>
        Customers.Entities.Customer.SyncFromCrm(1, 100, LifecycleStage.Lead,
            CustomerName.Create("Test", null, "User"), null, [], null, null, null, null);
}
