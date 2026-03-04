using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.EventBus.Emb;

/// <summary>
/// Configuration options for the Enterprise Message Bus (EMB 2.0) publisher.
/// </summary>
/// <remarks>
/// Bound to Messaging:EmbProducer. Only used in non-development environments
/// where EmbEventBus replaces the in-memory event bus.
/// </remarks>
public sealed class EmbProducerOptions : IValidatableObject
{
    public const string SectionName = "Messaging:EmbProducer";

    /// <summary>
    /// The name of the EventBridge bus to publish to (e.g., "dev-rtl-internal-event-bridge").
    /// </summary>
    [Required]
    public string EventBus { get; set; } = string.Empty;

    /// <summary>
    /// Cost center identifier used by the EMB producer (e.g., "rtl").
    /// </summary>
    [Required]
    public string CostCenter { get; set; } = string.Empty;

    /// <summary>
    /// The event source identifier sent with each EventBridge event (e.g., "rtl").
    /// </summary>
    [Required]
    public string EventSource { get; set; } = string.Empty;

    /// <summary>
    /// Reporter identifier included in EMB message metadata (e.g., "rtl.core").
    /// </summary>
    [Required]
    public string Reporter { get; set; } = string.Empty;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(EventBus))
        {
            yield return new ValidationResult(
                "EventBus name is required for EMB publishing.",
                [nameof(EventBus)]);
        }

        if (string.IsNullOrWhiteSpace(CostCenter))
        {
            yield return new ValidationResult(
                "CostCenter is required for EMB publishing.",
                [nameof(CostCenter)]);
        }

        if (string.IsNullOrWhiteSpace(EventSource))
        {
            yield return new ValidationResult(
                "EventSource is required for EMB publishing.",
                [nameof(EventSource)]);
        }

        if (string.IsNullOrWhiteSpace(Reporter))
        {
            yield return new ValidationResult(
                "Reporter is required for EMB publishing.",
                [nameof(Reporter)]);
        }
    }
}
