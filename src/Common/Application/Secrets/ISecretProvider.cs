namespace Rtl.Core.Application.Secrets;

/// <summary>
/// Retrieves secrets from a secret store (e.g. AWS Secrets Manager).
/// Results are cached per key with a configurable TTL.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Returns the raw secret string for the given secret name.
    /// </summary>
    Task<string> GetSecretStringAsync(string secretName, CancellationToken ct = default);

    /// <summary>
    /// Retrieves a secret and deserializes it from JSON to <typeparamref name="T"/>.
    /// </summary>
    Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default) where T : class;
}
