using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.Caching;
using System.Buffers;
using System.Text.Json;

namespace Rtl.Core.Infrastructure.Caching;

/// <summary>
/// Redis-based implementation of ICacheService.
/// </summary>
internal sealed class CacheService(
    IDistributedCache cache,
    IOptions<CachingOptions> options) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        byte[]? bytes = await cache.GetAsync(key, cancellationToken);

        return bytes is null ? default : Deserialize<T>(bytes);
    }

    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        byte[] bytes = Serialize(value);
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(options.Value.DefaultExpirationMinutes)
        };

        return cache.SetAsync(key, bytes, cacheEntryOptions, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        cache.RemoveAsync(key, cancellationToken);

    private static T Deserialize<T>(byte[] bytes) =>
        JsonSerializer.Deserialize<T>(bytes)!;

    private static byte[] Serialize<T>(T value)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(buffer);
        JsonSerializer.Serialize(writer, value);
        return buffer.WrittenSpan.ToArray();
    }
}
