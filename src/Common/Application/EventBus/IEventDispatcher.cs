namespace ModularTemplate.Application.EventBus;

/// <summary>
/// Dispatches integration events to their registered handlers.
/// </summary>
/// <remarks>
/// <para>
/// This interface abstracts the process of deserializing integration events
/// and routing them to the appropriate handlers. It supports both in-memory
/// and message queue-based implementations.
/// </para>
/// <para>
/// Implementations should resolve handlers from the dependency injection container
/// and invoke each handler for the given event type.
/// </para>
/// </remarks>
public interface IEventDispatcher
{
    /// <summary>
    /// Dispatches an integration event to all registered handlers.
    /// </summary>
    /// <param name="eventType">The fully qualified type name of the event.</param>
    /// <param name="eventJson">The JSON-serialized event payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DispatchAsync(string eventType, string eventJson, CancellationToken cancellationToken = default);
}
