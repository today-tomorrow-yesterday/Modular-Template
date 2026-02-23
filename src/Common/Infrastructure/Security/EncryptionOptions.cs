using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.Security;

public sealed class EncryptionOptions : IValidatableObject
{
    public const string SectionName = "Encryption";

    public string Key { get; set; } = string.Empty;

    public string KeyId { get; set; } = "1"; // Default version

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Key))
        {
            yield return new ValidationResult(
                "Encryption key is required. Set via 'Encryption:Key' config or ENCRYPTION_KEY environment variable.",
                [nameof(Key)]);
        }
    }
}
