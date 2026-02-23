using Microsoft.Extensions.DependencyInjection;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Infrastructure.Caching;
using System.Collections.Concurrent;
using System.Reflection;

namespace Rtl.Core.Infrastructure.Inbox.Handlers;

/// <summary>
/// Factory for resolving integration event handlers from assemblies.
/// </summary>
public static class IntegrationEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IIntegrationEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        var integrationEventHandlerTypes = HandlersDictionary.GetOrAdd(
            CacheKeys.Create(assembly.GetName().Name!, type.Name),
            _ =>
            {
                var handlerTypes = assembly.GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler<>).MakeGenericType(type)))
                    .ToArray();

                return handlerTypes;
            });

        List<IIntegrationEventHandler> handlers = [];
        foreach (Type integrationEventHandlerType in integrationEventHandlerTypes)
        {
            var integrationEventHandler = serviceProvider.GetRequiredService(integrationEventHandlerType);
            handlers.Add((integrationEventHandler as IIntegrationEventHandler)!);
        }

        return handlers;
    }
}
