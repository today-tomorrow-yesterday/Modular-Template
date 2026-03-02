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
    /// Secrets Manager secret name or ARN containing RDS credentials.
    /// </summary>
    [Required]
    public string SecretName { get; init; } = string.Empty;

    /// <summary>
    /// Target database name. Overrides the dbname field from the RDS secret,
    /// which is the RDS cluster default and not necessarily the application database.
    /// </summary>
    [Required]
    public string DatabaseName { get; init; } = string.Empty;

    /// <summary>
    /// SSL mode for deployed database connections. Defaults to Require.
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

        if (string.IsNullOrWhiteSpace(DatabaseName))
        {
            yield return new ValidationResult(
                "DatabaseName is required when DatabaseSecret section is configured.",
                [nameof(DatabaseName)]);
        }
    }
}
