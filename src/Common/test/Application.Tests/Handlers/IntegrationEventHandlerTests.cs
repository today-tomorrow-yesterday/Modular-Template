using Rtl.Core.Application.EventBus;
using Xunit;

namespace Rtl.Core.Application.Tests.Handlers;

public class IntegrationEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithCorrectEventType_CallsTypedHandleAsync()
    {
        var handler = new TestIntegrationEventHandler();
        var integrationEvent = new TestIntegrationEvent(Guid.NewGuid(), "test-data");

        await handler.HandleAsync((IIntegrationEvent)integrationEvent, CancellationToken.None);

        Assert.Equal(integrationEvent, handler.HandledEvent);
    }

    [Fact]
    public async Task HandleAsync_WithIncorrectEventType_ThrowsInvalidOperationException()
    {
        var handler = new TestIntegrationEventHandler();
        var wrongEvent = new OtherIntegrationEvent();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync((IIntegrationEvent)wrongEvent, CancellationToken.None));
    }

    private sealed record TestIntegrationEvent(Guid Id, string Data) : IIntegrationEvent
    {
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private sealed record OtherIntegrationEvent : IIntegrationEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

    private sealed class TestIntegrationEventHandler : IntegrationEventHandler<TestIntegrationEvent>
    {
        public TestIntegrationEvent? HandledEvent { get; private set; }

        public override Task HandleAsync(TestIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            HandledEvent = integrationEvent;
            return Task.CompletedTask;
        }
    }
}
