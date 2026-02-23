using Modules.SampleOrders.Domain.Orders.Events;
using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Results;
using Rtl.Core.Domain.ValueObjects;

namespace Modules.SampleOrders.Domain.Orders;

public sealed class Order : SoftDeletableEntity, IAggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    private Order() {}

    public int CustomerId { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money TotalPrice => CalculateTotal();

    public OrderStatus Status { get; private set; }

    public DateTime OrderedAtUtc { get; private set; }

    public static Result<Order> Place(int customerId)
    {
        if (customerId <= 0)
        {
            return Result.Failure<Order>(OrderErrors.CustomerRequired);
        }

        var order = new Order
        {
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            OrderedAtUtc = DateTime.UtcNow
        };

        order.Raise(new OrderPlacedDomainEvent());

        return order;
    }

    public Result AddLine(int productId, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.CannotModifyNonPendingOrder);
        }

        if (quantity <= 0)
        {
            return Result.Failure(OrderErrors.QuantityInvalid);
        }

        var existingLine = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (existingLine is not null)
        {
            existingLine.UpdateQuantity(existingLine.Quantity + quantity);
        }
        else
        {
            var line = OrderLine.Create(Id, productId, quantity, unitPrice);
            _lines.Add(line);

            Raise(new OrderLineAddedDomainEvent(productId, quantity));
        }

        return Result.Success();
    }

    public Result RemoveLine(int productId)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.CannotModifyNonPendingOrder);
        }

        var line = _lines.FirstOrDefault(l => l.ProductId == productId);
        if (line is null)
        {
            return Result.Failure(OrderErrors.LineNotFound);
        }

        _lines.Remove(line);

        Raise(new OrderLineRemovedDomainEvent(productId));

        return Result.Success();
    }

    public Result UpdateStatus(OrderStatus newStatus)
    {
        if (!IsValidStatusTransition(Status, newStatus))
        {
            return Result.Failure(OrderErrors.InvalidStatusTransition);
        }

        var oldStatus = Status;
        Status = newStatus;

        Raise(new OrderStatusChangedDomainEvent(oldStatus, newStatus));

        return Result.Success();
    }

    private Money CalculateTotal()
    {
        if (_lines.Count == 0)
        {
            return Money.Zero();
        }

        var currency = _lines[0].UnitPrice.Currency;
        var total = Money.Zero(currency);

        foreach (var line in _lines)
        {
            var addResult = total.Add(line.LineTotal);
            if (addResult.IsSuccess)
            {
                total = addResult.Value;
            }
        }

        return total;
    }

    private static bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
    {
        return (from, to) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Pending, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            _ => false
        };
    }
}
