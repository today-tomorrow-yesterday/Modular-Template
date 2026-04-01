namespace ModularTemplate.Application.EventBus;

/// <summary>
/// Abstraction for publishing integration events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an integration event.
    /// </summary>
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent;
}
