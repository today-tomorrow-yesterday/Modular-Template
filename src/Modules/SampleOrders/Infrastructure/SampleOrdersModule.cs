using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.SampleOrders.Application;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Domain.Customers;
using Modules.SampleOrders.Domain.Orders;
using Modules.SampleOrders.Domain.ProductsCache;
using Modules.SampleOrders.Infrastructure.EventBus;
using Modules.SampleOrders.Infrastructure.Persistence;
using Modules.SampleOrders.Infrastructure.Persistence.Repositories;
using Modules.SampleOrders.Presentation.IntegrationEvents;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.SampleOrders.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.SampleOrders.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.SampleOrders.Infrastructure;

public static class SampleOrdersModule
{
    public static IServiceCollection AddSampleOrdersModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<ISampleOrdersModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<OrdersDbContext>(databaseConnectionString, Schemas.Orders);

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductCacheRepository, ProductCacheRepository>();
        services.AddScoped<IProductCacheWriter, ProductCacheRepository>();
        services.AddScoped<IUnitOfWork<ISampleOrdersModule>>(sp => sp.GetRequiredService<OrdersDbContext>());

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
