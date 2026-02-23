using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.Exceptions;
using Rtl.Core.Infrastructure.Serialization;

namespace Rtl.Core.Infrastructure.EventBus.Aws;

/// <summary>
/// AWS EventBridge implementation of <see cref="IEventBus"/>.
/// </summary>
/// <remarks>
/// <para>
/// This implementation publishes integration events to AWS EventBridge for
/// cross-module communication in the modular monolith architecture.
/// </para>
/// <para>
/// Events are serialized to JSON and published with their fully qualified type name
/// as the DetailType, enabling consumers to route and deserialize events appropriately.
/// </para>
/// </remarks>
internal sealed class EventBridgeEventBus(
    IAmazonEventBridge eventBridgeClient,
    IOptions<AwsMessagingOptions> options,
    ILogger<EventBridgeEventBus> logger) : IEventBus
{
    private readonly AwsMessagingOptions _options = options.Value;

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var eventType = typeof(T).AssemblyQualifiedName!;
        var eventJson = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance);

        logger.LogDebug(
            "Publishing integration event {EventType} with ID {EventId} to EventBridge bus {EventBusName}",
            typeof(T).Name,
            integrationEvent.Id,
            _options.EventBusName);

        var request = new PutEventsRequest
        {
            Entries =
            [
                new PutEventsRequestEntry
                {
                    EventBusName = _options.EventBusName,
                    Source = _options.EventSource,
                    DetailType = eventType,
                    Detail = eventJson,
                    Time = DateTime.UtcNow
                }
            ]
        };

        var response = await eventBridgeClient.PutEventsAsync(request, cancellationToken);

        if (response.FailedEntryCount > 0)
        {
            var failedEntry = response.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.ErrorCode));
            var errorMessage = failedEntry is not null
                ? $"{failedEntry.ErrorCode}: {failedEntry.ErrorMessage}"
                : "Unknown error";

            logger.LogError(
                "Failed to publish integration event {EventType} with ID {EventId} to EventBridge: {ErrorMessage}",
                typeof(T).Name,
                integrationEvent.Id,
                errorMessage);

            throw new EventPublishException(typeof(T).Name, errorMessage);
        }

        logger.LogDebug(
            "Successfully published integration event {EventType} with ID {EventId} to EventBridge",
            typeof(T).Name,
            integrationEvent.Id);
    }
}
