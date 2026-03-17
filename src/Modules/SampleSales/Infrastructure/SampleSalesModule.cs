using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.SampleSales.Application;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Domain.Catalogs;
using Modules.SampleSales.Domain.OrdersCache;
using Modules.SampleSales.Domain.Products;
using Modules.SampleSales.Infrastructure.EventBus;
using Modules.SampleSales.Infrastructure.Persistence;
using Modules.SampleSales.Infrastructure.Persistence.Repositories;
using Modules.SampleSales.Presentation.IntegrationEvents;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.SampleSales.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.SampleSales.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.SampleSales.Infrastructure;

public static class SampleSalesModule
{
    public static IServiceCollection AddSampleSalesModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<ISampleSalesModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<SampleDbContext>(databaseConnectionString, Schemas.Sample);

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IOrderCacheRepository, OrderCacheRepository>();
        services.AddScoped<IOrderCacheWriter, OrderCacheRepository>();
        services.AddScoped<IUnitOfWork<ISampleSalesModule>>(sp => sp.GetRequiredService<SampleDbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Integration event handlers (Presentation assembly) + domain event handlers (Application assembly)
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);

        // SQS polling (disabled in development)
        services.AddSqsPolling<ProcessSqsJob>(environment);

        // Outbox pattern
        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection("Messaging:Outbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessOutboxJob<ProcessOutboxJob>>();

        // Inbox pattern
        services.AddOptions<InboxOptions>()
            .Bind(configuration.GetSection("Messaging:Inbox"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.ConfigureOptions<ConfigureProcessInboxJob<ProcessInboxJob>>();

        return services;
    }
}
