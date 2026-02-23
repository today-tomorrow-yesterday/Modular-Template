using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// Configuration options for the secret provider (AWS Secrets Manager caching).
/// </summary>
public sealed class SecretProviderOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name for secret provider options.
    /// </summary>
    public const string SectionName = "Secrets";

    /// <summary>
    /// Gets the duration in minutes to cache each secret in memory before re-fetching.
    /// </summary>
    [Range(1, 60)]
    public int CacheDurationMinutes { get; init; } = 5;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CacheDurationMinutes <= 0)
        {
            yield return new ValidationResult(
                "CacheDurationMinutes must be positive.",
                [nameof(CacheDurationMinutes)]);
        }
    }
}
