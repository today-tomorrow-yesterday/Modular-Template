using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.EventBus.Aws;

/// <summary>
/// Configuration options for AWS EventBridge and SQS messaging.
/// </summary>
/// <remarks>
/// These options control how the application publishes events to EventBridge
/// and consumes events from SQS queues.
/// </remarks>
public sealed class AwsMessagingOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name for AWS messaging options.
    /// </summary>
    public const string SectionName = "Messaging:AwsMessaging";

    /// <summary>
    /// Gets or sets the name of the EventBridge event bus.
    /// </summary>
    /// <remarks>
    /// If not explicitly configured, this is derived from <c>Application:ShortName</c>
    /// with the pattern: {shortname}-events (e.g., "retail-core-events").
    /// </remarks>
    public string EventBusName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source identifier for events published to EventBridge.
    /// </summary>
    /// <remarks>
    /// If not explicitly configured, this is derived from <c>Application:Name</c>
    /// in lowercase (e.g., "Rtl.Core").
    /// </remarks>
    public string EventSource { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of the SQS queue for this module to consume events from.
    /// </summary>
    public string SqsQueueUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets the interval in seconds between SQS queue polls.
    /// </summary>
    public int PollingIntervalSeconds { get; init; }

    /// <summary>
    /// Gets the maximum number of messages to retrieve per poll.
    /// </summary>
    public int MaxMessages { get; init; }

    /// <summary>
    /// Gets the visibility timeout in seconds for received messages.
    /// </summary>
    /// <remarks>
    /// This determines how long a message is invisible to other consumers
    /// after being received.
    /// </remarks>
    public int VisibilityTimeoutSeconds { get; init; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // PollingIntervalSeconds, MaxMessages, VisibilityTimeoutSeconds are validated
        // only if SqsQueueUrl is configured (SQS is optional)
        if (!string.IsNullOrWhiteSpace(SqsQueueUrl))
        {
            if (PollingIntervalSeconds <= 0)
            {
                yield return new ValidationResult(
                    "PollingIntervalSeconds must be positive when SQS is configured.",
                    [nameof(PollingIntervalSeconds)]);
            }

            if (MaxMessages <= 0 || MaxMessages > 10)
            {
                yield return new ValidationResult(
                    "MaxMessages must be between 1 and 10.",
                    [nameof(MaxMessages)]);
            }

            if (VisibilityTimeoutSeconds <= 0)
            {
                yield return new ValidationResult(
                    "VisibilityTimeoutSeconds must be positive.",
                    [nameof(VisibilityTimeoutSeconds)]);
            }
        }
    }
}
