using Modules.SampleOrders.Domain.ValueObjects;
using Rtl.Core.Domain.Entities;

namespace Modules.SampleOrders.Domain.Customers;

public sealed class CustomerAddress : Entity
{
    private CustomerAddress() { }

    public int CustomerId { get; private set; }

    public Address Address { get; private set; } = null!;

    public bool IsPrimary { get; private set; }

    internal static CustomerAddress Create(
        int customerId,
        Address address,
        bool isPrimary)
    {
        return new CustomerAddress
        {
            CustomerId = customerId,
            Address = address,
            IsPrimary = isPrimary
        };
    }

    internal void Update(Address address)
    {
        Address = address;
    }
}
