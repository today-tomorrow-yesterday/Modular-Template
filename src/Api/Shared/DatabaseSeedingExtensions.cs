using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularTemplate.Application.Seeding;

namespace ModularTemplate.Api.Shared;

public static class DatabaseSeedingExtensions
{
    public static async Task<IApplicationBuilder> SeedDataAsync(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration.GetSection(SeedingOptions.SectionName)
            .Get<SeedingOptions>() ?? new SeedingOptions();

        if (!options.Enabled)
        {
            return app;
        }

        using var scope = app.ApplicationServices.CreateScope();
        var seeders = scope.ServiceProvider
            .GetServices<IModuleSeeder>()
            .OrderBy(s => s.Order)
            .ToList();

        if (seeders.Count == 0)
        {
            return app;
        }

        Bogus.Randomizer.Seed = new Random(options.Seed);

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseSeeding");

        foreach (var seeder in seeders)
        {
            logger.LogInformation("Seeding {Module}...", seeder.ModuleName);
            await seeder.SeedAsync(scope.ServiceProvider, CancellationToken.None);
            logger.LogInformation("Seeding {Module} completed.", seeder.ModuleName);
        }

        return app;
    }
}
