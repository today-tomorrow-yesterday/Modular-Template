namespace ModularTemplate.Infrastructure.Caching;

/// <summary>
/// Constants for cache key formatting.
/// </summary>
internal static class CacheKeys
{
    /// <summary>
    /// Separator used between cache key segments.
    /// </summary>
    public const string Separator = ":";

    /// <summary>
    /// Creates a cache key from the given segments.
    /// </summary>
    public static string Create(params string[] segments) =>
        string.Join(Separator, segments);
}
