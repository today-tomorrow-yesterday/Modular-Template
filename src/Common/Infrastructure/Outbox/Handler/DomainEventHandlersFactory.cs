using Microsoft.Extensions.DependencyInjection;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Infrastructure.Caching;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularTemplate.Infrastructure.Outbox.Handler;

/// <summary>
/// Factory for resolving domain event handlers from assemblies.
/// </summary>
public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        var domainEventHandlerTypes = HandlersDictionary.GetOrAdd(
            CacheKeys.Create(assembly.GetName().Name!, type.Name),
            _ =>
            {
                Type[] handlerTypes = [.. assembly
                    .GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler<>)
                    .MakeGenericType(type)))];

                return handlerTypes;
            });

        List<IDomainEventHandler> handlers = [];
        foreach (var domainEventHandlerType in domainEventHandlerTypes)
        {
            var domainEventHandler = serviceProvider.GetRequiredService(domainEventHandlerType);
            handlers.Add((domainEventHandler as IDomainEventHandler)!);
        }

        return handlers;
    }
}
