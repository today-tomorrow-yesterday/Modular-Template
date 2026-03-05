using CMH.Common.EMBClient.Contracts;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.EventBus;

namespace Rtl.Core.Infrastructure.EventBus.Emb;

/// <summary>
/// EMB 2.0 implementation of IEventBus that wraps IEMBProducer
/// from CMH.Common.EMBClient.
/// </summary>
/// <remarks>
/// Uses <see cref="EventDetailTypeAttribute"/> to resolve a friendly detail-type
/// (e.g., "rtl.sales.deliveryAddressCreated"); falls back to AssemblyQualifiedName
/// for undecorated events.
/// </remarks>
internal sealed class EmbEventBus(
    IEMBProducer producer,
    IOptions<EmbProducerOptions> settings) : IEventBus
{
    private readonly EmbProducerOptions _settings = settings.Value;

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        var detailType = IntegrationEvent.GetDetailType(typeof(T));

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
