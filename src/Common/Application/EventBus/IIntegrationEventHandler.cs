namespace ModularTemplate.Application.EventBus;

/// <summary>
/// Non-generic base interface for integration event handlers.
/// </summary>
public interface IIntegrationEventHandler
{
    Task HandleAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}

/// <summary>
/// Handler for integration events.
/// </summary>
public interface IIntegrationEventHandler<in TIntegrationEvent>
    : IIntegrationEventHandler
    where TIntegrationEvent : IIntegrationEvent
{
    Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
