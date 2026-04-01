namespace ModularTemplate.Infrastructure.Security;

/// <summary>
/// Service for encrypting and decrypting sensitive data.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts the provided plain text.
    /// </summary>
    string Encrypt(string? plainText);

    /// <summary>
    /// Decrypts the provided cipher text.
    /// </summary>
    string Decrypt(string? cipherText);
}
