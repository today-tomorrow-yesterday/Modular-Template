using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularTemplate.Domain;

namespace ModularTemplate.Infrastructure.Clock;

/// <summary>
/// Extension methods for configuring clock services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds clock services to the service collection.
    /// </summary>
    internal static IServiceCollection AddClockServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}
