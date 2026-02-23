using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.ISeries;

#pragma warning disable IDE1006 // Naming Styles
internal sealed class iSeriesOptions : IValidatableObject
#pragma warning restore IDE1006 // Naming Styles
{
    public const string SectionName = "ISeries";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    // When null or empty, JWT auth is skipped (Development mode).
    public string? SigningKeySecretName { get; set; }

    public string ValidIssuer { get; set; } = "rtl-core";
    public string ValidAudience { get; set; } = "iseries-gateway";

    [Range(1, 60)]
    public int TokenLifetimeMinutes { get; set; } = 5;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
        {
            yield return new ValidationResult(
                "BaseUrl must be a valid absolute URI.",
                [nameof(BaseUrl)]);
        }
    }
}
