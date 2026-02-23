using Microsoft.Extensions.Logging;
using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// No-op provider for local development when AWS Secrets Manager is unavailable.
/// Returns empty strings for all secret requests.
/// </summary>
internal sealed class DevelopmentSecretProvider(
    ILogger<DevelopmentSecretProvider> logger) : ISecretProvider
{
    public Task<string> GetSecretStringAsync(string secretName, CancellationToken ct = default)
    {
        logger.LogDebug("Development mode — skipping secret fetch for '{SecretName}'", secretName);
        return Task.FromResult(string.Empty);
    }

    public Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default) where T : class
    {
        throw new InvalidOperationException(
            $"Cannot deserialize secret '{secretName}' in Development mode. " +
            "Configure the secret in appsettings.Development.json or register IAmazonSecretsManager.");
    }
}
