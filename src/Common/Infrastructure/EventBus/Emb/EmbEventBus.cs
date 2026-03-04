using CMH.Common.EMBClient.Contracts;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.EventBus;

namespace Rtl.Core.Infrastructure.EventBus.Emb;

/// <summary>
/// EMB 2.0 implementation of IEventBus that wraps IEMBProducer
/// from CMH.Common.EMBClient.
/// </summary>
/// <remarks>
/// Uses AssemblyQualifiedName as the DetailType so that the consumer-side
/// EventDispatcher can call Type.GetType(detailType) to resolve
/// the correct IIntegrationEventHandler.
///
/// This differs from the legacy Sales API which uses attribute-based routing
/// (e.g., "rtl.sales.saleCreated") — that pattern is incompatible with
/// our generic consumer dispatch.
/// </remarks>
internal sealed class EmbEventBus(
    IEMBProducer producer,
    IOptions<EmbProducerOptions> settings) : IEventBus
{
    private readonly EmbProducerOptions _settings = settings.Value;

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var detailType = typeof(T).AssemblyQualifiedName!;

        var embMessage = new EMBMessage<T>
        {
            Data = integrationEvent,
            Metadata = new EMBMessageMetadata
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = detailType,
                ForwardToEmb = false,
                Reporter = _settings.Reporter,
                Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            }
        };

        await producer.SendEventAsync(
            _settings.EventSource,
            detailType,
            embMessage);
    }
}
