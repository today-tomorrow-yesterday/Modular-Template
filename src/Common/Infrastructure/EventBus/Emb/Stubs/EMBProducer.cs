// Placeholder types for CMH.Common.EMBClient.Producer.
// Delete this file and add the real NuGet package when available.

using CMH.Common.EMBClient.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CMH.Common.EMBClient.Producer;

public class EMBProducerConfigurationOptions { }

public static class EMBProducerExtensions
{
    public static IServiceCollection AddEmbProducer(
        this IServiceCollection services, string costCenter, string eventBus)
    {
        // Registers a no-op producer. Replace with real package for production.
        services.AddSingleton<IEMBProducer, StubEMBProducer>();
        return services;
    }
}

internal sealed class StubEMBProducer(ILogger<StubEMBProducer> logger) : IEMBProducer
{
    public Task SendEventAsync<T>(string source, string detailType, EMBMessage<T> message)
    {
        logger.LogWarning(
            "StubEMBProducer: Would publish {DetailType} to {Source}. Install CMH.Common.EMBClient for real publishing.",
            detailType, source);
        return Task.CompletedTask;
    }
}
