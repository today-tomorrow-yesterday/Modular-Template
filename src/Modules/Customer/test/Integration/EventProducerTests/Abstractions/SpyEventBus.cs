using Rtl.Core.Application.EventBus;

namespace Modules.Customer.EventProducerTests.Abstractions;

// Captures all published integration events for assertion.
// Registered in DI to replace InMemoryEventBus -- events are captured, not dispatched.
public sealed class SpyEventBus : IEventBus
{
    private readonly List<IIntegrationEvent> _events = [];
    public IReadOnlyList<IIntegrationEvent> PublishedEvents => _events;

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        _events.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public void Clear() => _events.Clear();

    public T GetSingle<T>() where T : IIntegrationEvent
        => (T)_events.Single(e => e is T);

    public IEnumerable<T> GetAll<T>() where T : IIntegrationEvent
        => _events.OfType<T>();

    public bool HasEvent<T>() where T : IIntegrationEvent
        => _events.Any(e => e is T);
}
