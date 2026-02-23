using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.Secrets;

internal sealed class AwsSecretProvider(
    IAmazonSecretsManager secretsManager,
    IMemoryCache cache,
    IOptions<SecretProviderOptions> options,
    ILogger<AwsSecretProvider> logger) : ISecretProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<string> GetSecretStringAsync(string secretName, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        var cacheKey = $"secret:{secretName}";

        if (cache.TryGetValue<string>(cacheKey, out var cached))
            return cached!;

        logger.LogInformation("Fetching secret '{SecretName}' from AWS Secrets Manager", secretName);

        var response = await secretsManager.GetSecretValueAsync(
            new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT"
            }, ct);

        var ttl = TimeSpan.FromMinutes(options.Value.CacheDurationMinutes);
        cache.Set(cacheKey, response.SecretString, ttl);

        return response.SecretString;
    }

    public async Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default) where T : class
    {
        var json = await GetSecretStringAsync(secretName, ct);

        return JsonSerializer.Deserialize<T>(json, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Secret '{secretName}' deserialized to null for type {typeof(T).Name}.");
    }
}
