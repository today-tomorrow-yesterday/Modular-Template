using Amazon.EventBridge;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using CMH.Common.EMBClient.Producer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Infrastructure.EventBus.Aws;
using ModularTemplate.Infrastructure.EventBus.Emb;
using ModularTemplate.Infrastructure.EventBus.InMemory;
using ModularTemplate.Infrastructure.Resilience;
using System.Reflection;

namespace ModularTemplate.Infrastructure.EventBus;

/// <summary>
/// Extension methods for configuring messaging services.
/// </summary>
public static class Startup
{
    /// <summary>
    /// Adds messaging services configured for the current environment.
    /// Development: In-memory event bus (synchronous, no external dependencies)
    /// Production: EMB 2.0 (CMH.Common.EMBClient → EventBridge) + SQS (consuming)
    /// </summary>
    internal static IServiceCollection AddMessagingServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        // Always register the event dispatcher
        services.AddScoped<IEventDispatcher, EventDispatcher>();

        if (environment.IsDevelopment())
        {
            // Local development: In-memory, synchronous dispatch
            services.AddScoped<IEventBus, InMemoryEventBus>();
        }
        else
        {
            // --- SQS Consumer Options ---
            services.AddOptions<SqsConsumerOptions>()
                .Bind(configuration.GetSection(SqsConsumerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // --- EMB Producer Options ---
            services.AddOptions<EmbProducerOptions>()
                .Bind(configuration.GetSection(EmbProducerOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            // Register EMB producer config (required by CMH.Common.EMBClient internally)
            services.AddSingleton(new EMBProducerConfigurationOptions());

            // Resolve EMB producer settings and register EMB producer
            var embSettings = configuration.GetSection(EmbProducerOptions.SectionName).Get<EmbProducerOptions>()!;
            services.AddEmbProducer(embSettings.CostCenter, embSettings.EventBus);

            // Register AWS SDK clients
            services.AddAWSService<IAmazonEventBridge>();
            services.AddAWSService<IAmazonSQS>();
            services.AddSingleton<IAmazonSimpleSystemsManagement>(
                new AmazonSimpleSystemsManagementClient());

            // EMB publisher with resilience wrapper (retry + circuit breaker)
            services.AddScoped<EmbEventBus>();
            services.AddScoped<IEventBus>(sp =>
            {
                var innerEventBus = sp.GetRequiredService<EmbEventBus>();
                var resilienceOptions = sp.GetRequiredService<IOptions<ResilienceOptions>>();
                var logger = sp.GetRequiredService<ILogger<ResilientEventBusWrapper>>();
                return new ResilientEventBusWrapper(innerEventBus, resilienceOptions, logger);
            });
        }

        return services;
    }

    /// <summary>
    /// Adds SQS polling job for consuming events. Only active in non-development environments.
    /// </summary>
    /// <typeparam name="TJob">The SQS polling job type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="environment">The host environment.</param>
    /// <param name="pollingIntervalSeconds">Interval between SQS polls (default: 5 seconds).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqsPolling<TJob>(
        this IServiceCollection services,
        IHostEnvironment environment,
        int pollingIntervalSeconds = 5)
        where TJob : class, IJob
    {
        if (!environment.IsDevelopment())
        {
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey(typeof(TJob).Name);
                q.AddJob<TJob>(opts => opts
                    .WithIdentity(jobKey)
                    .StoreDurably()); // Required when using multiple AddQuartz calls
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity($"{typeof(TJob).Name}-trigger")
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(pollingIntervalSeconds)
                        .RepeatForever()));
            });
        }

        return services;
    }

    /// <summary>
    /// Registers integration event handlers from the specified assembly.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembly">The assembly containing integration event handlers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddIntegrationEventHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        // Find all types implementing IIntegrationEventHandler<T>
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>)));

        foreach (var handlerType in handlerTypes)
        {
            var interfaces = handlerType.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>));

            foreach (var @interface in interfaces)
            {
                services.AddScoped(@interface, handlerType);
            }
        }

        return services;
    }

    /// <summary>
    /// Registers domain event handlers from the specified assembly.
    /// Required because IDomainEventHandler&lt;T&gt; is not a MediatR type,
    /// so MediatR's RegisterServicesFromAssemblies does not discover them.
    /// The outbox processor resolves handlers via DomainEventHandlersFactory.GetHandlers(),
    /// which calls GetRequiredService(handlerType).
    /// </summary>
    public static IServiceCollection AddDomainEventHandlers(
        this IServiceCollection services,
        Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

        return services;
    }
}
