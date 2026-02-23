using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rtl.Core.Domain;

namespace Rtl.Core.Infrastructure.Clock;

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
