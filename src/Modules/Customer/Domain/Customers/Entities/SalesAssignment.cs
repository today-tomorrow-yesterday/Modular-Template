using Modules.Customer.Domain.Customers.Enums;
using Modules.Customer.Domain.SalesPersons;

namespace Modules.Customer.Domain.Customers.Entities;

// Join entity linking a Customer to a SalesPerson with a role (Primary, Supporting).
// Filtered unique index enforces exactly one Primary per Customer; multiple Supporting allowed.
public sealed class SalesAssignment
{
    private SalesAssignment() {}

    public int Id { get; private set; }
    public int CustomerId { get; private set; }
    public string SalesPersonId { get; private set; } = null!;
    public SalesAssignmentRole Role { get; private set; }

    public Customer Customer { get; private set; } = null!;
    public SalesPerson SalesPerson { get; private set; } = null!;

    internal static SalesAssignment Create(int customerId, string salesPersonId, SalesAssignmentRole role)
    {
        return new SalesAssignment
        {
            CustomerId = customerId,
            SalesPersonId = salesPersonId,
            Role = role
        };
    }
}
