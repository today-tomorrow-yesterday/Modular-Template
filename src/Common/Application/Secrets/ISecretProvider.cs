namespace Rtl.Core.Application.Secrets;

/// <summary>
/// Retrieves secrets from a secret store (e.g. AWS Secrets Manager).
/// Results are cached per key with a configurable TTL.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Retrieves a secret and deserializes it from JSON to <typeparamref name="T"/>.
    /// For raw string secrets, use <c>GetSecretAsync&lt;string&gt;</c>.
    /// </summary>
    Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default);
}
