using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rtl.Core.Application.FeatureManagement;

namespace Rtl.Core.Infrastructure.FeatureManagement;

/// <summary>
/// Extension methods for configuring feature flag services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds feature flag services to the service collection.
    /// </summary>
    internal static IServiceCollection AddFeatureManagementServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.TryAddSingleton<IFeatureFlagService>(sp =>
            new ConfigurationFeatureFlagService(configuration));
        return services;
    }
}
