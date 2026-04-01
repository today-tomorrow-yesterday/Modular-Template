using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Domain.Entities;

namespace Modules.SampleOrders.Domain.Orders;

public sealed class ShippingAddress : Entity
{
    private ShippingAddress() { }

    public int OrderId { get; private set; }

    public Address Address { get; private set; } = null!;

    internal static ShippingAddress Create(int orderId, Address address)
    {
        return new ShippingAddress
        {
            OrderId = orderId,
            Address = address
        };
    }

    internal void Update(Address address)
    {
        Address = address;
    }
}
