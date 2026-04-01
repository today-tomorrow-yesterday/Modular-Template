using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularTemplate.Application.Secrets;
using System.Text.Json;

namespace ModularTemplate.Infrastructure.Secrets;

/// <summary>
/// AWS Secrets Manager implementation of <see cref="ISecretProvider"/>
/// with per-key in-memory caching.
/// Also owns the shared <see cref="AmazonSecretsManagerClient"/> used at startup
/// by <see cref="DatabaseConnectionResolver"/> (before DI is available).
/// </summary>
internal sealed class AwsSecretProvider(
    IAmazonSecretsManager secretsManager,
    IMemoryCache cache,
    IOptions<SecretProviderOptions> options,
    ILogger<AwsSecretProvider> logger) : ISecretProvider
{
    private static readonly Lazy<AmazonSecretsManagerClient> DefaultClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// The shared client instance. Registered in DI and used by
    /// <see cref="DatabaseConnectionResolver"/> at startup — one client for the lifetime of the app.
    /// </summary>
    internal static IAmazonSecretsManager SharedClient => DefaultClient.Value;

    /// <summary>
    /// Fetches a raw secret string using the shared client.
    /// Called by <see cref="DatabaseConnectionResolver"/> at startup before DI is available.
    /// </summary>
    internal static Task<string> FetchSecretStringAsync(
        string secretName, CancellationToken ct = default)
        => FetchCoreAsync(SharedClient, secretName, ct);

    /// <inheritdoc />
    public async Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default)
    {
        var raw = await GetCachedSecretStringAsync(secretName, ct);

        if (typeof(T) == typeof(string))
            return (T)(object)raw;

        return JsonSerializer.Deserialize<T>(raw, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Secret '{secretName}' deserialized to null for type {typeof(T).Name}.");
    }

    private async Task<string> GetCachedSecretStringAsync(string secretName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        var cacheKey = $"secret:{secretName}";

        if (cache.TryGetValue<string>(cacheKey, out var cached))
            return cached!;

        logger.LogInformation("Fetching secret '{SecretName}' from AWS Secrets Manager", secretName);

        var secretString = await FetchCoreAsync(secretsManager, secretName, ct);

        var ttl = TimeSpan.FromMinutes(options.Value.CacheDurationMinutes);
        cache.Set(cacheKey, secretString, ttl);

        return secretString;
    }

    /// <summary>
    /// Core AWS fetch with standardized exception handling.
    /// </summary>
    private static async Task<string> FetchCoreAsync(
        IAmazonSecretsManager client,
        string secretName,
        CancellationToken ct = default)
    {
        try
        {
            var response = await client.GetSecretValueAsync(
                new GetSecretValueRequest
                {
                    SecretId = secretName,
                    VersionStage = "AWSCURRENT"
                }, ct);

            return response.SecretString;
        }
        catch (ResourceNotFoundException ex)
        {
            throw new InvalidOperationException(
                $"Secret '{secretName}' was not found in AWS Secrets Manager.", ex);
        }
        catch (DecryptionFailureException ex)
        {
            throw new InvalidOperationException(
                $"Failed to decrypt secret '{secretName}' — verify KMS key permissions.", ex);
        }
        catch (InvalidRequestException ex)
        {
            throw new InvalidOperationException(
                $"Invalid request for secret '{secretName}' — it may be pending deletion.", ex);
        }
        catch (InvalidParameterException ex)
        {
            throw new InvalidOperationException(
                $"Invalid parameter when fetching secret '{secretName}'.", ex);
        }
        catch (InternalServiceErrorException ex)
        {
            throw new InvalidOperationException(
                $"AWS Secrets Manager internal error fetching secret '{secretName}'.", ex);
        }
    }
}
