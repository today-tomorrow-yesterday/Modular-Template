namespace Rtl.Core.Application.Caching;

/// <summary>
/// Abstraction for caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache.
    /// </summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in cache.
    /// </summary>
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from cache.
    /// </summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}
