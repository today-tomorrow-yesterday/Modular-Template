using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.ValueObjects;

namespace Modules.SampleOrders.Domain.Orders;

/// <summary>
/// Entity representing a line item within an Order aggregate.
/// Not an aggregate root - can only be accessed through the Order aggregate.
/// </summary>
public sealed class OrderLine : Entity
{
    private OrderLine() {}

    public int OrderId { get; private set; }

    public int ProductId { get; private set; }

    public int Quantity { get; private set; }

    public Money UnitPrice { get; private set; } = null!;

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    internal static OrderLine Create(int orderId, int productId, int quantity, Money unitPrice)
    {
        return new OrderLine
        {
            OrderId = orderId,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }

    internal void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}
