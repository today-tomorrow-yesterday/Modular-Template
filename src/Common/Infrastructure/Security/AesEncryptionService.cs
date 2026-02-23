using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Rtl.Core.Infrastructure.Security;

/// <summary>
/// Provides AES-256-GCM encryption using a key from Options or Environment Variable.
/// Supports Key Versioning by prepending "{KeyId}:" to the ciphertext.
/// </summary>
internal sealed class AesEncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const char Separator = ':';

    private readonly byte[] _key;
    private readonly string _keyId;

    public AesEncryptionService(IOptions<EncryptionOptions>? options)
    {
        // Fail Fast: Validate key on startup
        var keyString = options?.Value?.Key;
        _keyId = options?.Value?.KeyId ?? "1";

        if (string.IsNullOrEmpty(keyString))
        {
            keyString = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");
        }

        if (string.IsNullOrEmpty(keyString))
        {
            throw new InvalidOperationException("Encryption key not found. Configure 'Encryption:Key' or set 'ENCRYPTION_KEY' env var.");
        }

        try
        {
            _key = Convert.FromBase64String(keyString);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("Encryption key is not valid Base64.");
        }

        if (_key.Length != 32) // AES-256 requirement
        {
            throw new InvalidOperationException($"Encryption key must be 32 bytes (256 bits). Current length: {_key.Length} bytes.");
        }
    }

    public string Encrypt(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Format: Nonce + Cipher + Tag
        var payload = new byte[NonceSize + cipherBytes.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, payload, 0, NonceSize);
        Buffer.BlockCopy(cipherBytes, 0, payload, NonceSize, cipherBytes.Length);
        Buffer.BlockCopy(tag, 0, payload, NonceSize + cipherBytes.Length, TagSize);

        // Return Versioned Ciphertext: "v1:base64..."
        return $"{_keyId}{Separator}{Convert.ToBase64String(payload)}";
    }

    public string Decrypt(string? cipherText)
    {
        if (string.IsNullOrEmpty(cipherText)) return string.Empty;

        // Parse Version
        ReadOnlySpan<char> span = cipherText.AsSpan();
        var separatorIdx = span.IndexOf(Separator);

        ReadOnlySpan<char> payloadSpan;
        
        if (separatorIdx > 0)
        {
            // var version = span.Slice(0, separatorIdx); // Future: Use this to select key
            payloadSpan = span.Slice(separatorIdx + 1);
        }
        else
        {
            // Legacy/No-Version fallback (assume current key)
            payloadSpan = span;
        }

        byte[] fullCipher = Convert.FromBase64String(payloadSpan.ToString());
        
        if (fullCipher.Length < NonceSize + TagSize)
            throw new ArgumentException("Invalid cipher text length.");

        using var aes = new AesGcm(_key, TagSize);

        var nonce = fullCipher.AsSpan(0, NonceSize);
        var tag = fullCipher.AsSpan(fullCipher.Length - TagSize, TagSize);
        var cipher = fullCipher.AsSpan(NonceSize, fullCipher.Length - NonceSize - TagSize);
        
        var plainBytes = new byte[cipher.Length];
        aes.Decrypt(nonce, cipher, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
