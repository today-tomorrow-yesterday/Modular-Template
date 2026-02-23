using Rtl.Core.Application.Messaging;
using Rtl.Core.Domain.Events;
using Xunit;

namespace Rtl.Core.Application.Tests.Handlers;

public class DomainEventHandlerTests
{
    [Fact]
    public async Task Handle_WithCorrectEventType_CallsTypedHandle()
    {
        var handler = new TestDomainEventHandler();
        var domainEvent = new TestDomainEvent("test-data");

        await handler.Handle((IDomainEvent)domainEvent, CancellationToken.None);

        Assert.Equal(domainEvent, handler.HandledEvent);
    }

    [Fact]
    public async Task Handle_WithIncorrectEventType_ThrowsInvalidOperationException()
    {
        var handler = new TestDomainEventHandler();
        var wrongEvent = new OtherDomainEvent();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle((IDomainEvent)wrongEvent, CancellationToken.None));
    }

    private sealed record TestDomainEvent(string Data) : DomainEvent;
    private sealed record OtherDomainEvent : DomainEvent;

    private sealed class TestDomainEventHandler : DomainEventHandler<TestDomainEvent>
    {
        public TestDomainEvent? HandledEvent { get; private set; }

        public override Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            HandledEvent = domainEvent;
            return Task.CompletedTask;
        }
    }
}
