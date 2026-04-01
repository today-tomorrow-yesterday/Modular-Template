using Microsoft.Extensions.Logging;
using ModularTemplate.Application.Secrets;

namespace ModularTemplate.Infrastructure.Secrets;

/// <summary>
/// No-op provider for local development when AWS Secrets Manager is unavailable.
/// Returns empty strings for string secrets; throws for typed secrets.
/// </summary>
internal sealed class DevelopmentSecretProvider(
    ILogger<DevelopmentSecretProvider> logger) : ISecretProvider
{
    public Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default)
    {
        if (typeof(T) == typeof(string))
        {
            logger.LogDebug("Development mode — skipping secret fetch for '{SecretName}'", secretName);
            return Task.FromResult((T)(object)string.Empty);
        }

        throw new InvalidOperationException(
            $"Cannot deserialize secret '{secretName}' in Development mode. " +
            "Configure the secret in appsettings.Development.json or register IAmazonSecretsManager.");
    }
}
