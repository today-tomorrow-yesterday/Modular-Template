using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// AWS Secrets Manager implementation of <see cref="ISecretProvider"/>
/// with per-key in-memory caching.
/// </summary>
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

    /// <inheritdoc />
    public async Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default)
    {
        var raw = await GetSecretStringAsync(secretName, ct);

        if (typeof(T) == typeof(string))
            return (T)(object)raw;

        return JsonSerializer.Deserialize<T>(raw, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Secret '{secretName}' deserialized to null for type {typeof(T).Name}.");
    }

    private async Task<string> GetSecretStringAsync(string secretName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName);

        var cacheKey = $"secret:{secretName}";

        if (cache.TryGetValue<string>(cacheKey, out var cached))
            return cached!;

        logger.LogInformation("Fetching secret '{SecretName}' from AWS Secrets Manager", secretName);

        try
        {
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
        catch (ResourceNotFoundException ex)
        {
            logger.LogError(ex, "Secret '{SecretName}' was not found in AWS Secrets Manager", secretName);
            throw;
        }
        catch (DecryptionFailureException ex)
        {
            logger.LogError(ex, "Failed to decrypt secret '{SecretName}' — verify KMS key permissions", secretName);
            throw;
        }
        catch (InvalidRequestException ex)
        {
            logger.LogError(ex, "Invalid request for secret '{SecretName}' — it may be pending deletion", secretName);
            throw;
        }
        catch (InvalidParameterException ex)
        {
            logger.LogError(ex, "Invalid parameter when fetching secret '{SecretName}'", secretName);
            throw;
        }
        catch (InternalServiceErrorException ex)
        {
            logger.LogError(ex, "AWS Secrets Manager internal error fetching '{SecretName}'", secretName);
            throw;
        }
    }
}
