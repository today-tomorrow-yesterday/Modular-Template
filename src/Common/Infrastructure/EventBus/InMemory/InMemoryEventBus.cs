using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Infrastructure.Serialization;

namespace Rtl.Core.Infrastructure.EventBus.InMemory;

/// <summary>
/// In-memory implementation of IEventBus for local development.
/// </summary>
/// <remarks>
/// <para>
/// This implementation serializes events to JSON and dispatches them synchronously
/// via <see cref="IEventDispatcher"/>. It is intended for local development and
/// testing scenarios where external messaging infrastructure (e.g., AWS EventBridge/SQS)
/// is not required.
/// </para>
/// <para>
/// Events are dispatched immediately within the same process, making it easier to
/// debug and test integration event flows without external dependencies.
/// </para>
/// </remarks>
internal sealed class InMemoryEventBus(
    IEventDispatcher dispatcher,
    ILogger<InMemoryEventBus> logger) : IEventBus
{
    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var eventType = IntegrationEvent.GetDetailType(typeof(T));
        var eventJson = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance);

        logger.LogDebug(
            "Publishing integration event {EventType} with ID {EventId} via in-memory event bus",
            typeof(T).Name,
            integrationEvent.Id);

        await dispatcher.DispatchAsync(eventType, eventJson, cancellationToken);

        logger.LogDebug(
            "Successfully dispatched integration event {EventType} with ID {EventId}",
            typeof(T).Name,
            integrationEvent.Id);
    }
}
