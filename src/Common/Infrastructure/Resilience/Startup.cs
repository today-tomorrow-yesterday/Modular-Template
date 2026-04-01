using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularTemplate.Infrastructure.Resilience;

/// <summary>
/// Extension methods for configuring resilience services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds resilience services and options to the service collection.
    /// </summary>
    internal static IServiceCollection AddResilienceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ResilienceOptions>()
            .Bind(configuration.GetSection(ResilienceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
