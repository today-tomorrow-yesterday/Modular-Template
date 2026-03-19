using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modules.Sales.Application;
using Modules.Sales.Domain;
using Modules.Sales.Domain.AuthorizedUsersCache;
using Modules.Sales.Domain.Cdc;
using Modules.Sales.Domain.DeliveryAddresses;
using Modules.Sales.Domain.FundingCache;
using Modules.Sales.Domain.InventoryCache;
using Modules.Sales.Domain.Packages;
using Modules.Sales.Domain.CustomersCache;
using Modules.Sales.Domain.RetailLocations;
using Modules.Sales.Domain.Sales;
using Modules.Sales.Infrastructure.EventBus;
using Modules.Sales.Infrastructure.Persistence;
using Modules.Sales.Infrastructure.Persistence.Repositories;
using Modules.Sales.Infrastructure.Seeding;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Application.Seeding;
using Rtl.Core.Infrastructure;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Infrastructure.Inbox.Job;
using Rtl.Core.Infrastructure.Outbox.Job;
using Rtl.Core.Infrastructure.Persistence;
using ProcessInboxJob = Modules.Sales.Infrastructure.Inbox.ProcessInboxJob;
using ProcessOutboxJob = Modules.Sales.Infrastructure.Outbox.ProcessOutboxJob;

namespace Modules.Sales.Infrastructure;

public static class SalesModule
{
    public static IServiceCollection AddSalesModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string databaseConnectionString)
    {
        services
            .AddModuleDataSource<ISalesModule>(databaseConnectionString)
            .AddPersistence(databaseConnectionString)
            .AddMessaging(configuration, environment);

        services.AddScoped<IModuleSeeder, SalesModuleSeeder>();

        return services;
    }

    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string databaseConnectionString)
    {
        services.AddModuleDbContext<SalesDbContext>(databaseConnectionString, Schemas.Sales);

        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPackageRepository, PackageRepository>();
        services.AddScoped<ICustomerCacheRepository, CustomerCacheRepository>();
        services.AddScoped<ICustomerCacheWriter, CustomerCacheRepository>();
        services.AddScoped<IAuthorizedUserCacheRepository, AuthorizedUserCacheRepository>();
        services.AddScoped<IAuthorizedUserCacheWriter, AuthorizedUserCacheRepository>();
        services.AddScoped<IFundingRequestCacheRepository, FundingRequestCacheRepository>();
        services.AddScoped<IFundingRequestCacheWriter, FundingRequestCacheRepository>();
        services.AddScoped<IOnLotHomeCacheWriter, OnLotHomeCacheRepository>();
        services.AddScoped<ILandParcelCacheWriter, LandParcelCacheRepository>();
        services.AddScoped<IInventoryCacheQueries, InventoryCacheQueries>();
        services.AddScoped<ICdcTaxQueries, CdcTaxQueries>();
        services.AddScoped<ICdcPricingQueries, CdcPricingQueries>();
        services.AddScoped<ICdcProjectCostQueries, CdcProjectCostQueries>();
        services.AddScoped<IRetailLocationRepository, RetailLocationRepository>();
        services.AddScoped<IDeliveryAddressRepository, DeliveryAddressRepository>();
        services.AddScoped<ISaleNumberGenerator, SaleNumberGenerator>();
        services.AddScoped<IUnitOfWork<ISalesModule>>(sp => sp.GetRequiredService<SalesDbContext>());

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
