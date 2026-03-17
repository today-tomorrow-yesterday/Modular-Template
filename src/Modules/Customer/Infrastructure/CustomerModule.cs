using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Customer.Application;
using Modules.Customer.Application.Parties.OnboardPersonFromLoan;
using Modules.Customer.Domain;
using Modules.Customer.Domain.Parties;
using Modules.Customer.Domain.SalesPersons;
using Modules.Customer.Infrastructure.Adapters;
using Modules.Customer.Infrastructure.EventBus;
using Modules.Customer.Infrastructure.Persistence;
using Modules.Customer.Infrastructure.Persistence.Repositories;
using Modules.Customer.Infrastructure.Seeding;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Application.Seeding;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.Customer.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.Customer.Infrastructure;

public static class CustomerModule
{
    public static IServiceCollection AddCustomerModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<ICustomerModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        services.AddScoped<IModuleSeeder, CustomerModuleSeeder>();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<CustomerDbContext>(databaseConnectionString, Schemas.Customers);

        services.AddScoped<IPartyRepository, PartyRepository>();
        services.AddScoped<ISalesPersonRepository, SalesPersonRepository>();
        services.AddScoped<IUnitOfWork<ICustomerModule>>(sp => sp.GetRequiredService<CustomerDbContext>());

        // VMF LOS adapter - stubbed for now, replace with real implementation
        services.AddScoped<IVmfLosAdapter, StubVmfLosAdapter>();

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
