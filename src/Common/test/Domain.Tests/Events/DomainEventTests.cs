using Rtl.Core.Domain.Events;
using Xunit;

namespace Rtl.Core.Domain.Tests.Events;

public class DomainEventTests
{
    [Fact]
    public void DefaultConstructor_GeneratesIdAndTimestamp()
    {
        var domainEvent = new TestDomainEvent();

        Assert.NotEqual(Guid.Empty, domainEvent.Id);
        Assert.True(domainEvent.OccurredOnUtc <= DateTime.UtcNow);
    }

    [Fact]
    public void ParameterizedConstructor_SetsProvidedValues()
    {
        var id = Guid.NewGuid();
        var occurredOnUtc = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var domainEvent = new TestDomainEventWithConstructor(id, occurredOnUtc);

        Assert.Equal(id, domainEvent.Id);
        Assert.Equal(occurredOnUtc, domainEvent.OccurredOnUtc);
    }

    private sealed record TestDomainEvent : DomainEvent;

    private sealed record TestDomainEventWithConstructor(Guid Id, DateTime OccurredOnUtc)
        : DomainEvent(Id, OccurredOnUtc);
}
