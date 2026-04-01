namespace ModularTemplate.Domain.Auditing;

/// <summary>
/// Marks a property as sensitive. 
/// 1. The database column will be encrypted.
/// 2. The audit log will store the CIPHERTEXT (not the plaintext, and not masked).
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class SensitiveDataAttribute : Attribute;
