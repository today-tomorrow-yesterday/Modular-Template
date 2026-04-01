using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace ModularTemplate.Infrastructure.Security;

/// <summary>
/// Encrypts any type T by serializing to JSON, then encrypting.
/// </summary>
public sealed class JsonEncryptionValueConverter<T> : ValueConverter<T, string>
{
    public JsonEncryptionValueConverter(IEncryptionService encryptionService, ConverterMappingHints? mappingHints = null)
        : base(
            v => encryptionService.Encrypt(JsonSerializer.Serialize(v, JsonSerializerOptions.Default)),
            v => JsonSerializer.Deserialize<T>(encryptionService.Decrypt(v), JsonSerializerOptions.Default)!,
            mappingHints)
    {
    }

    // Default constructor for EF Core design-time tooling (migrations, model snapshots).
    [Obsolete("Use dependency injection constructor. This is for EF Core design-time use only.")]
    public JsonEncryptionValueConverter() : this(CreateDesignTimeFallback())
    {
    }

    private static IEncryptionService CreateDesignTimeFallback()
    {
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        if (string.IsNullOrEmpty(key))
        {
            key = Convert.ToBase64String(new byte[32]);
        }

        return new AesEncryptionService(Options.Create(new EncryptionOptions { Key = key, KeyId = "design-time" }));
    }
}
