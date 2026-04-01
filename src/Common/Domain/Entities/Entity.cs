using ModularTemplate.Domain.Events;

namespace ModularTemplate.Domain.Entities;

/// <summary>
/// Base class for all domain entities with domain event support.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Unique identifier for the entity, assigned by the database via Hi-Lo sequence.
    /// </summary>
    public int Id { get; protected set; }

    /// <summary>
    /// Collection of domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary>
    /// Clears all domain events from the entity.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Raises a new domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
