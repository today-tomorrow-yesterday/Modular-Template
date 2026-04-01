using ModularTemplate.Domain.Events;

namespace ModularTemplate.Application.Messaging;

/// <summary>
/// Base class for domain event handlers that provides type-safe handling.
/// </summary>
public abstract class DomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public abstract Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default);

    public Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        if (domainEvent is not TDomainEvent typedEvent)
        {
            throw new InvalidOperationException(
                $"Cannot handle domain event of type '{domainEvent.GetType().Name}'. " +
                $"Expected type '{typeof(TDomainEvent).Name}'.");
        }

        return Handle(typedEvent, cancellationToken);
    }
}
