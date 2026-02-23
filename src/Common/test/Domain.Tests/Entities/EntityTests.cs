using Rtl.Core.Domain.Entities;
using Rtl.Core.Domain.Events;
using Xunit;

namespace Rtl.Core.Domain.Tests.Entities;

public class EntityTests
{
    [Fact]
    public void Raise_AddsEventToDomainEvents()
    {
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        entity.RaiseEvent(domainEvent);

        Assert.Single(entity.DomainEvents);
        Assert.Contains(domainEvent, entity.DomainEvents);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var entity = new TestEntity();
        entity.RaiseEvent(new TestDomainEvent());

        entity.ClearDomainEvents();

        Assert.Empty(entity.DomainEvents);
    }

    private sealed class TestEntity : Entity
    {
        public void RaiseEvent(IDomainEvent domainEvent) => Raise(domainEvent);
    }

    private sealed record TestDomainEvent : DomainEvent;
}
