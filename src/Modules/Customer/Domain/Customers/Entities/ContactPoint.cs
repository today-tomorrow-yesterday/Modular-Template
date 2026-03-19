using Modules.Customer.Domain.Customers.Enums;
using Rtl.Core.Domain.Auditing;
using Rtl.Core.Domain.Entities;

namespace Modules.Customer.Domain.Customers.Entities;

// Contact method for a Customer. Simplified Type + Value model (sufficient for retail bounded context).
public sealed class ContactPoint : Entity
{
    private ContactPoint() {}

    public int CustomerId { get; private set; }
    public ContactPointType Type { get; private set; }
    [SensitiveData] public string Value { get; private set; } = null!;
    public bool IsPrimary { get; private set; }

    internal static ContactPoint Create(int customerId, ContactPointType type, string value, bool isPrimary = false)
    {
        return new ContactPoint
        {
            CustomerId = customerId,
            Type = type,
            Value = value,
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    internal void UpdateValue(string value) => Value = value;
}
