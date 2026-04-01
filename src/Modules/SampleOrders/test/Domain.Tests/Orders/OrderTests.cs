using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.Orders.Events;
using ModularTemplate.Domain.ValueObjects;
using Xunit;

namespace Modules.SampleOrders.Domain.Tests.Orders;

public sealed class OrderTests
{
    // ─── Place ────────────────────────────────────────────────────

    [Fact]
    public void Place_returns_success_with_valid_customerId()
    {
        // Arrange & Act
        var result = Order.Place(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.CustomerId);
        Assert.Equal(OrderStatus.Pending, result.Value.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Place_returns_failure_when_customerId_is_zero_or_negative(int customerId)
    {
        // Arrange & Act
        var result = Order.Place(customerId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.CustomerRequired, result.Error);
    }

    [Fact]
    public void Place_generates_PublicId_and_raises_OrderPlacedDomainEvent()
    {
        // Arrange & Act
        var result = Order.Place(1);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.PublicId);
        Assert.Contains(
            result.Value.DomainEvents,
            e => e is OrderPlacedDomainEvent);
    }

    // ─── AddProductLine ──────────────────────────────────────────

    [Fact]
    public void AddProductLine_adds_line_and_raises_event()
    {
        // Arrange
        var order = Order.Place(1).Value;
        order.ClearDomainEvents();
        var unitPrice = Money.Create(10.00m).Value;

        // Act
        var result = order.AddProductLine(2, unitPrice, productCacheId: 5);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(order.Lines);
        Assert.Contains(
            order.DomainEvents,
            e => e is OrderLineAddedDomainEvent);
    }

    [Fact]
    public void AddProductLine_returns_failure_when_not_pending()
    {
        // Arrange
        var order = Order.Place(1).Value;
        order.UpdateStatus(OrderStatus.Confirmed);
        var unitPrice = Money.Create(10.00m).Value;

        // Act
        var result = order.AddProductLine(1, unitPrice);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.CannotModifyNonPendingOrder, result.Error);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddProductLine_returns_failure_when_quantity_invalid(int quantity)
    {
        // Arrange
        var order = Order.Place(1).Value;
        var unitPrice = Money.Create(10.00m).Value;

        // Act
        var result = order.AddProductLine(quantity, unitPrice);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.QuantityInvalid, result.Error);
    }

    // ─── AddCustomLine ───────────────────────────────────────────

    [Fact]
    public void AddCustomLine_adds_line_and_raises_event()
    {
        // Arrange
        var order = Order.Place(1).Value;
        order.ClearDomainEvents();
        var unitPrice = Money.Create(25.00m).Value;

        // Act
        var result = order.AddCustomLine(3, unitPrice);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(order.Lines);
        Assert.Contains(
            order.DomainEvents,
            e => e is OrderLineAddedDomainEvent);
    }

    // ─── RemoveLine ──────────────────────────────────────────────

    [Fact]
    public void RemoveLine_removes_existing_line()
    {
        // Arrange
        var order = Order.Place(1).Value;
        var unitPrice = Money.Create(10.00m).Value;
        order.AddProductLine(1, unitPrice);
        var lineId = order.Lines.First().Id;
        order.ClearDomainEvents();

        // Act
        var result = order.RemoveLine(lineId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(order.Lines);
        Assert.Contains(
            order.DomainEvents,
            e => e is OrderLineRemovedDomainEvent);
    }

    [Fact]
    public void RemoveLine_returns_failure_when_line_not_found()
    {
        // Arrange
        var order = Order.Place(1).Value;

        // Act
        var result = order.RemoveLine(999);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.LineNotFound, result.Error);
    }

    // ─── UpdateStatus ────────────────────────────────────────────

    [Fact]
    public void UpdateStatus_transitions_pending_to_confirmed()
    {
        // Arrange
        var order = Order.Place(1).Value;
        order.ClearDomainEvents();

        // Act
        var result = order.UpdateStatus(OrderStatus.Confirmed);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
        Assert.Contains(
            order.DomainEvents,
            e => e is OrderStatusChangedDomainEvent);
    }

    [Fact]
    public void UpdateStatus_rejects_invalid_transition()
    {
        // Arrange — Pending cannot go directly to Delivered
        var order = Order.Place(1).Value;

        // Act
        var result = order.UpdateStatus(OrderStatus.Delivered);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
    }

    [Fact]
    public void UpdateStatus_rejects_transition_from_delivered()
    {
        // Arrange — Delivered is a terminal state
        var order = Order.Place(1).Value;
        order.UpdateStatus(OrderStatus.Confirmed);
        order.UpdateStatus(OrderStatus.Shipped);
        order.UpdateStatus(OrderStatus.Delivered);

        // Act
        var result = order.UpdateStatus(OrderStatus.Cancelled);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(OrderErrors.InvalidStatusTransition, result.Error);
    }
}
