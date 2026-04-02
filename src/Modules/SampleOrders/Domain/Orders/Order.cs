using Modules.SampleOrders.Domain.Orders.Events;
using Modules.SampleOrders.Domain.ValueObjects;
using ModularTemplate.Domain.Entities;
using ModularTemplate.Domain.Results;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleOrders.Domain.Orders;

public sealed class Order : SoftDeletableEntity, IAggregateRoot
{
    private readonly List<OrderLine> _lines = [];

    private Order() { }

    public Guid PublicId { get; private set; }

    public int CustomerId { get; private set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money TotalPrice => CalculateTotal();

    public OrderStatus Status { get; private set; }

    public DateTime OrderedAtUtc { get; private set; }

    public ShippingAddress? ShippingAddress { get; private set; }

    // ─── Factory Methods ───────────────────────────────────────────

    public static Result<Order> Place(int customerId)
    {
        if (customerId <= 0)
        {
            return Result.Failure<Order>(OrderErrors.CustomerRequired);
        }

        var order = new Order
        {
            PublicId = Guid.CreateVersion7(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            OrderedAtUtc = DateTime.UtcNow
        };

        order.Raise(new OrderPlacedDomainEvent());

        return order;
    }

    // ─── Line Management ──────────────────────────────────────────

    public Result AddProductLine(
        int quantity,
        Money unitPrice,
        int? productCacheId = null,
        ProductLineDetails? details = null)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.CannotModifyNonPendingOrder);
        }

        if (quantity <= 0)
        {
            return Result.Failure(OrderErrors.QuantityInvalid);
        }

        var line = ProductLine.Create(Id, quantity, unitPrice, productCacheId, details, _lines.Count);
        _lines.Add(line);

        Raise(new OrderLineAddedDomainEvent(line.Id));

        return Result.Success();
    }

    public Result AddCustomLine(
        int quantity,
        Money unitPrice,
        CustomLineDetails? details = null)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.CannotModifyNonPendingOrder);
        }

        if (quantity <= 0)
        {
            return Result.Failure(OrderErrors.QuantityInvalid);
        }

        var line = CustomLine.Create(Id, quantity, unitPrice, details, _lines.Count);
        _lines.Add(line);

        Raise(new OrderLineAddedDomainEvent(line.Id));

        return Result.Success();
    }

    public Result RemoveLine(int lineId)
    {
        if (Status != OrderStatus.Pending)
        {
            return Result.Failure(OrderErrors.CannotModifyNonPendingOrder);
        }

        var line = _lines.FirstOrDefault(l => l.Id == lineId);
        if (line is null)
        {
            return Result.Failure(OrderErrors.LineNotFound);
        }

        _lines.Remove(line);

        Raise(new OrderLineRemovedDomainEvent(lineId));

        return Result.Success();
    }

    // ─── Status Transitions ───────────────────────────────────────

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

    // ─── Shipping Address ─────────────────────────────────────────

    public Result SetShippingAddress(Address address)
    {
        if (ShippingAddress is null)
        {
            ShippingAddress = ShippingAddress.Create(Id, address);
            Raise(new ShippingAddressCreatedDomainEvent());
        }
        else
        {
            ShippingAddress.Update(address);
            Raise(new ShippingAddressChangedDomainEvent());
        }

        return Result.Success();
    }

    // ─── Private Helpers ──────────────────────────────────────────

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
