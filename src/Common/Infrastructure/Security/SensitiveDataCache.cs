using ModularTemplate.Domain.Auditing;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularTemplate.Infrastructure.Security;

/// <summary>
/// Caches properties marked with [SensitiveData] to avoid reflection in hot paths.
/// </summary>
public static class SensitiveDataCache
{
    private static readonly ConcurrentDictionary<Type, HashSet<string>> _cache = new();

    public static bool IsSensitive(Type entityType, string propertyName)
    {
        var sensitiveProps = _cache.GetOrAdd(entityType, type =>
        {
            return [.. type.GetProperties()
                .Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() is not null)
                .Select(p => p.Name)];
        });

        return sensitiveProps.Contains(propertyName);
    }
}
