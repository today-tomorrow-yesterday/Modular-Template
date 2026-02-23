using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Organization.Application;
using Modules.Organization.Domain;
using Modules.Organization.Domain.HomeCenters;
using Modules.Organization.Domain.ManualAssignments;
using Modules.Organization.Domain.Regions;
using Modules.Organization.Domain.Users;
using Modules.Organization.Domain.Zones;
using Modules.Organization.Infrastructure.EventBus;
using Modules.Organization.Infrastructure.Persistence;
using Modules.Organization.Infrastructure.Persistence.Repositories;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.Organization.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.Organization.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.Organization.Infrastructure;

public static class OrganizationModule
{
    public static IServiceCollection AddOrganizationModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<IOrganizationModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<OrganizationDbContext>(databaseConnectionString, Schemas.Organizations);

        services.AddScoped<IHomeCenterRepository, HomeCenterRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserHomeCenterRepository, UserHomeCenterRepository>();
        services.AddScoped<IRegionRepository, RegionRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();
        services.AddScoped<IManualHomeCenterAssignmentRepository, ManualHomeCenterAssignmentRepository>();
        services.AddScoped<IManualZoneAssignmentRepository, ManualZoneAssignmentRepository>();
        services.AddScoped<IUnitOfWork<IOrganizationModule>>(sp => sp.GetRequiredService<OrganizationDbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIntegrationEventHandlers(AssemblyReference.Assembly);
        services.AddSqsPolling<ProcessSqsJob>(environment);

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection("Messaging:Outbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessOutboxJob<ProcessOutboxJob>>();

        services.AddOptions<InboxOptions>()
            .Bind(configuration.GetSection("Messaging:Inbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessInboxJob<ProcessInboxJob>>();

        return services;
    }
}
