using System.ComponentModel.DataAnnotations;

namespace ModularTemplate.Infrastructure.EventBus.Aws;

/// <summary>
/// Configuration options for SQS message consumption.
/// </summary>
/// <remarks>
/// These options control how the application consumes events from SQS queues.
/// Publishing is handled separately via <see cref="Emb.EmbProducerOptions"/> and the EMB 2.0 producer.
/// </remarks>
public sealed class SqsConsumerOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name for SQS consumer options.
    /// </summary>
    public const string SectionName = "Messaging:SqsConsumer";

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
