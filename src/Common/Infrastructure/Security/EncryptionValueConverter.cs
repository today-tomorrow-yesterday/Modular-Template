using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Options;

namespace ModularTemplate.Infrastructure.Security;

/// <summary>
/// EF Core Value Converter that encrypts data before writing to DB and decrypts on read.
/// Uses Randomized Encryption (AES-GCM), so deterministic searching is NOT possible.
/// </summary>
public sealed class EncryptionValueConverter(IEncryptionService encryptionService, ConverterMappingHints? mappingHints = null)
    : ValueConverter<string, string>(
        v => encryptionService.Encrypt(v),
        v => encryptionService.Decrypt(v),
        mappingHints)
{

    // Default constructor for EF Core design-time tooling (migrations, model snapshots).
    // Provides a dummy key so the converter can be instantiated for schema generation.
    // Never used at runtime — DI constructor is used instead.
    [Obsolete("Use dependency injection constructor. This is for EF Core design-time use only.")]
    public EncryptionValueConverter() : this(CreateDesignTimeFallback())
    {
    }

    private static IEncryptionService CreateDesignTimeFallback()
    {
        // Try real key from env var first, fall back to dummy key for design-time
        var key = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        if (string.IsNullOrEmpty(key))
        {
            key = Convert.ToBase64String(new byte[32]); // 256-bit zero key — design-time only
        }

        return new AesEncryptionService(Options.Create(new EncryptionOptions { Key = key, KeyId = "design-time" }));
    }
}
