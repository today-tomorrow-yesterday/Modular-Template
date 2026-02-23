using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.Tests.ISeries;

/// <summary>
/// Hand-rolled fake for ISecretProvider. Returns configured secret string or throws configured exception.
/// </summary>
internal sealed class FakeSecretProvider : ISecretProvider
{
    public string SecretToReturn { get; set; } = string.Empty;
    public Exception? ExceptionToThrow { get; set; }
    public int CallCount { get; private set; }
    public string? LastSecretName { get; private set; }

    public Task<T> GetSecretAsync<T>(string secretName, CancellationToken ct = default)
    {
        CallCount++;
        LastSecretName = secretName;

        if (ExceptionToThrow is not null)
            throw ExceptionToThrow;

        if (typeof(T) == typeof(string))
            return Task.FromResult((T)(object)SecretToReturn);

        throw new NotImplementedException(
            $"FakeSecretProvider only supports string secrets, not {typeof(T).Name}.");
    }
}
