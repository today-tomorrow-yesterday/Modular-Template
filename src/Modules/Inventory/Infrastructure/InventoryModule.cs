using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Inventory.Domain;
using Modules.Inventory.Domain.HomeCentersCache;
using Modules.Inventory.Domain.LandCosts;
using Modules.Inventory.Domain.LandParcels;
using Modules.Inventory.Domain.OnLotHomes;
using Modules.Inventory.Domain.SaleSummariesCache;
using Modules.Inventory.Domain.WheelsAndAxles;
using Modules.Inventory.Infrastructure.EventBus;
using Modules.Inventory.Infrastructure.Persistence;
using Modules.Inventory.Infrastructure.Persistence.Repositories;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.Inventory.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.Inventory.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.Inventory.Infrastructure;

public static class InventoryModule
{
    public static IServiceCollection AddInventoryModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<IInventoryModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<InventoryDbContext>(databaseConnectionString, Schemas.Inventories);

        services.AddScoped<IOnLotHomeRepository, OnLotHomeRepository>();
        services.AddScoped<ILandParcelRepository, LandParcelRepository>();
        services.AddScoped<ILandCostRepository, LandCostRepository>();
        services.AddScoped<Domain.AncillaryData.IAncillaryDataRepository, AncillaryDataRepository>();
        services.AddScoped<IWheelsAndAxlesTransactionRepository, WheelsAndAxlesTransactionRepository>();
        services.AddScoped<ISaleSummaryCacheRepository, SaleSummaryCacheRepository>();
        services.AddScoped<ISaleSummaryCacheWriter, SaleSummaryCacheRepository>();
        services.AddScoped<IHomeCenterCacheRepository, HomeCenterCacheRepository>();
        services.AddScoped<IHomeCenterCacheWriter, HomeCenterCacheRepository>();
        services.AddScoped<IUnitOfWork<IInventoryModule>>(sp => sp.GetRequiredService<InventoryDbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);

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
