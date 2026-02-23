using Modules.Customer.Domain.Parties.Entities;
using Modules.Customer.Domain.Parties.Enums;
using Xunit;

namespace Modules.Customer.Domain.Tests;

public sealed class PersonSalesAssignmentTests
{
    [Fact]
    public void Assigning_Primary_replaces_the_existing_Primary()
    {
        var person = CreateTestPerson();

        person.AssignSalesPerson("SP-1", SalesAssignmentRole.Primary);
        person.AssignSalesPerson("SP-2", SalesAssignmentRole.Primary);

        Assert.Equal("SP-2", person.GetPrimarySalesPersonId());
        Assert.DoesNotContain(person.SalesAssignments, sa => sa.SalesPersonId == "SP-1");
    }

    [Fact]
    public void Assigning_same_SalesPerson_twice_is_idempotent()
    {
        var person = CreateTestPerson();

        person.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);
        person.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);

        Assert.Single(person.SalesAssignments);
    }

    [Fact]
    public void Multiple_Supporting_assignments_are_allowed()
    {
        var person = CreateTestPerson();

        person.AssignSalesPerson("SP-1", SalesAssignmentRole.Supporting);
        person.AssignSalesPerson("SP-2", SalesAssignmentRole.Supporting);
        person.AssignSalesPerson("SP-3", SalesAssignmentRole.Primary);

        Assert.Equal(3, person.SalesAssignments.Count);
        Assert.Equal("SP-3", person.GetPrimarySalesPersonId());
    }

    [Fact]
    public void RemoveSalesAssignment_removes_by_SalesPersonId()
    {
        var person = CreateTestPerson();
        person.AssignSalesPerson("SP-1", SalesAssignmentRole.Primary);
        person.AssignSalesPerson("SP-2", SalesAssignmentRole.Supporting);

        person.RemoveSalesAssignment("SP-1");

        Assert.Single(person.SalesAssignments);
        Assert.Null(person.GetPrimarySalesPersonId());
    }

    private static Person CreateTestPerson() =>
        Person.SyncFromCrm(1, 100, LifecycleStage.Lead,
            PersonName.Create("Test", null, "User"), null, [], null, null, null, null);
}
