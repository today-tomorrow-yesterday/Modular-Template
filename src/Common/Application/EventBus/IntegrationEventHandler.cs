namespace Rtl.Core.Application.EventBus;

/// <summary>
/// Base class for integration event handlers that provides type-safe handling.
/// </summary>
/// <typeparam name="TIntegrationEvent">The type of integration event.</typeparam>
public abstract class IntegrationEventHandler<TIntegrationEvent>
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
{
    public abstract Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);

    public Task HandleAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        if (integrationEvent is not TIntegrationEvent typedEvent)
        {
            throw new InvalidOperationException(
                $"Cannot handle integration event of type '{integrationEvent.GetType().Name}'. " +
                $"Expected type '{typeof(TIntegrationEvent).Name}'.");
        }

        return HandleAsync(typedEvent, cancellationToken);
    }
}
