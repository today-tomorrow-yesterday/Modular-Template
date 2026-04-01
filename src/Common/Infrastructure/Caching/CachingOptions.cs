using System.ComponentModel.DataAnnotations;

namespace ModularTemplate.Infrastructure.Caching;

/// <summary>
/// Configuration options for distributed caching.
/// </summary>
public sealed class CachingOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name for caching options.
    /// </summary>
    public const string SectionName = "Caching";

    /// <summary>
    /// Gets the default cache expiration time in minutes.
    /// </summary>
    public int DefaultExpirationMinutes { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DefaultExpirationMinutes <= 0)
        {
            yield return new ValidationResult(
                "DefaultExpirationMinutes must be positive.",
                [nameof(DefaultExpirationMinutes)]);
        }
    }
}
