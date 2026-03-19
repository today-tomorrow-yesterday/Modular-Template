using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Domain.Entities;

namespace Modules.Customer.Domain.Customers.Entities;

// External system identifier for a Customer. One per type (unique constraint on customer_id + type).
public sealed class CustomerIdentifier : Entity
{
    private CustomerIdentifier() {}

    public int CustomerId { get; private set; }
    public IdentifierType Type { get; private set; }
    public string Value { get; private set; } = null!;

    internal static CustomerIdentifier Create(int customerId, IdentifierType type, string value)
    {
        return new CustomerIdentifier
        {
            CustomerId = customerId,
            Type = type,
            Value = value
        };
    }

    internal void UpdateValue(string value) => Value = value;
}
