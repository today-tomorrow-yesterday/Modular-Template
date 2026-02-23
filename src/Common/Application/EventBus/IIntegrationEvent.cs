namespace Rtl.Core.Application.EventBus;

/// <summary>
/// Marker interface for integration events (cross-module communication).
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Unique identifier of the integration event.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    DateTime OccurredOnUtc { get; }
}
