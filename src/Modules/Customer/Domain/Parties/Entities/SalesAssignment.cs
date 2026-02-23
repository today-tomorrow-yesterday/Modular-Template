using Modules.Customer.Domain.Parties.Enums;
using Modules.Customer.Domain.SalesPersons;

namespace Modules.Customer.Domain.Parties.Entities;

// Join entity linking a Person to a SalesPerson with a role (Primary, Supporting).
// Filtered unique index enforces exactly one Primary per Person; multiple Supporting allowed.
public sealed class SalesAssignment
{
    private SalesAssignment() {}

    public int Id { get; private set; }
    public int PersonId { get; private set; }
    public string SalesPersonId { get; private set; } = null!;
    public SalesAssignmentRole Role { get; private set; }

    public Person Person { get; private set; } = null!;
    public SalesPerson SalesPerson { get; private set; } = null!;

    internal static SalesAssignment Create(int personId, string salesPersonId, SalesAssignmentRole role)
    {
        return new SalesAssignment
        {
            PersonId = personId,
            SalesPersonId = salesPersonId,
            Role = role
        };
    }
}
