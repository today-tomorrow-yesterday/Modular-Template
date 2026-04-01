using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace ModularTemplate.Infrastructure.Quartz;

/// <summary>
/// Extension methods for configuring Quartz scheduler services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds Quartz scheduler services to the service collection.
    /// </summary>
    internal static IServiceCollection AddQuartzServices(this IServiceCollection services)
    {
        services.AddQuartz(configurator =>
        {
            var scheduler = Guid.NewGuid();
            configurator.SchedulerId = $"default-id-{scheduler}";
            configurator.SchedulerName = $"default-name-{scheduler}";
        });

        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
        return services;
    }
}
