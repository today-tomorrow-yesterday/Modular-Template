using ModularTemplate.Domain.Events;

namespace ModularTemplate.Application.Messaging;

/// <summary>
/// Non-generic base interface for domain event handlers.
/// </summary>
public interface IDomainEventHandler
{
    Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for domain events.
/// </summary>
public interface IDomainEventHandler<in TDomainEvent> : IDomainEventHandler
    where TDomainEvent : IDomainEvent
{
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
