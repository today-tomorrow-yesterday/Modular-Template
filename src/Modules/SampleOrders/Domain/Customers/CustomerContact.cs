using ModularTemplate.Domain.Auditing;
using ModularTemplate.Domain.Entities;

namespace Modules.SampleOrders.Domain.Customers;

public enum ContactType
{
    Email,
    Phone,
    MobilePhone
}

public sealed class CustomerContact : Entity
{
    private CustomerContact() { }

    public int CustomerId { get; private set; }

    public ContactType Type { get; private set; }

    [SensitiveData]
    public string Value { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    internal static CustomerContact Create(int customerId, ContactType type, string value, bool isPrimary)
    {
        return new CustomerContact
        {
            CustomerId = customerId,
            Type = type,
            Value = value,
            IsPrimary = isPrimary
        };
    }

    internal void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}
