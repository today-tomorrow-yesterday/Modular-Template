namespace Rtl.Core.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }

    /// <summary>
    /// The ID of the source entity that raised this event.
    /// Set by the outbox interceptor after the entity's ID is assigned.
    /// </summary>
    int EntityId { get; init; }

    DateTime OccurredOnUtc { get; }
}
