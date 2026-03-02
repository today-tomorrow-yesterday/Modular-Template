using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// Configuration options for resolving database credentials from AWS Secrets Manager.
/// Bound to the <c>DatabaseSecret</c> configuration section.
/// </summary>
public sealed class DatabaseSecretOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "DatabaseSecret";

    /// <summary>
    /// Gets the Secrets Manager secret name or ARN containing RDS credentials.
    /// </summary>
    [Required]
    public string SecretName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SSL mode for deployed database connections.
    /// Defaults to <c>Require</c>.
    /// </summary>
    public string SslMode { get; init; } = "Require";

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(SecretName))
        {
            yield return new ValidationResult(
                "SecretName is required when DatabaseSecret section is configured.",
                [nameof(SecretName)]);
        }
    }
}
