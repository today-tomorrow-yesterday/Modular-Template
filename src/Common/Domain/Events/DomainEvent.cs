namespace Rtl.Core.Domain.Events;

/// <summary>
/// Base class for domain events with automatic ID and timestamp generation.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        Id = Guid.NewGuid();
        OccurredOnUtc = DateTime.UtcNow;
    }

    protected DomainEvent(Guid id, DateTime occurredOnUtc)
    {
        Id = id;
        OccurredOnUtc = occurredOnUtc;
    }

    public Guid Id { get; init; }

    public int EntityId { get; init; }

    public DateTime OccurredOnUtc { get; init; }
}
