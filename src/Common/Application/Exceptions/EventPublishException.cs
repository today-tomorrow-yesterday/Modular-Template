namespace Rtl.Core.Application.Exceptions;

/// <summary>
/// Exception thrown when an event fails to publish to the event bus.
/// </summary>
public sealed class EventPublishException(string eventType, string? message)
    : Exception($"Failed to publish event '{eventType}': {message}")
{
    /// <summary>
    /// Gets the type of event that failed to publish.
    /// </summary>
    public string EventType { get; } = eventType;
}
