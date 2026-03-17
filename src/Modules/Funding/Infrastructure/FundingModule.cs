using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Funding.Application;
using Modules.Funding.Domain;
using Modules.Funding.Domain.CustomersCache;
using Modules.Funding.Domain.FundingRequests;
using Modules.Funding.Infrastructure.EventBus;
using Modules.Funding.Infrastructure.Persistence;
using Modules.Funding.Infrastructure.Persistence.Repositories;
using Modules.Funding.Presentation.IntegrationEvents;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.Funding.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.Funding.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.Funding.Infrastructure;

public static class FundingModule
{
    public static IServiceCollection AddFundingModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<IFundingModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<FundingDbContext>(databaseConnectionString, Schemas.Fundings);

        services.AddScoped<ICustomerCacheRepository, CustomerCacheRepository>();
        services.AddScoped<ICustomerCacheWriter, CustomerCacheRepository>();
        services.AddScoped<IFundingRequestRepository, FundingRequestRepository>();
        services.AddScoped<IPendingFundingRequestRepository, PendingFundingRequestRepository>();
        services.AddScoped<IUnitOfWork<IFundingModule>>(sp => sp.GetRequiredService<FundingDbContext>());

        return services;
    }

    private static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddIntegrationEventHandlers(Presentation.AssemblyReference.Assembly);
        services.AddDomainEventHandlers(Application.AssemblyReference.Assembly);

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
