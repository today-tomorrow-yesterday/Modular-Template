using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Infrastructure.Serialization;

namespace Rtl.Core.Infrastructure.EventBus;

/// <summary>
/// Dispatches integration events to their registered handlers by resolving them from DI.
/// </summary>
/// <remarks>
/// <para>
/// This implementation deserializes the event JSON to the correct type and invokes
/// all registered handlers for that event type. Handlers are resolved from the
/// service provider using the generic <see cref="IIntegrationEventHandler{TEvent}"/> interface.
/// </para>
/// <para>
/// Errors during handler invocation are logged but do not prevent other handlers
/// from being invoked, ensuring fault isolation between handlers.
/// </para>
/// </remarks>
internal sealed class EventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<EventDispatcher> logger) : IEventDispatcher
{
    public async Task DispatchAsync(string eventType, string eventJson, CancellationToken cancellationToken = default)
    {
        var type = Type.GetType(eventType);
        if (type is null)
        {
            logger.LogWarning("Could not resolve event type: {EventType}", eventType);
            return;
        }

        var @event = JsonConvert.DeserializeObject(eventJson, type, SerializerSettings.Instance);
        if (@event is null)
        {
            logger.LogWarning("Could not deserialize event of type: {EventType}", eventType);
            return;
        }

        var handlerInterfaceType = typeof(IIntegrationEventHandler<>).MakeGenericType(type);
        var handlers = serviceProvider.GetServices(handlerInterfaceType);

        foreach (var handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            try
            {
                // Use the non-generic interface to invoke HandleAsync
                if (handler is IIntegrationEventHandler integrationEventHandler)
                {
                    await integrationEventHandler.HandleAsync((IIntegrationEvent)@event, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Error handling integration event {EventType} with handler {HandlerType}",
                    eventType,
                    handler.GetType().Name);

                // Continue processing other handlers
            }
        }
    }
}
