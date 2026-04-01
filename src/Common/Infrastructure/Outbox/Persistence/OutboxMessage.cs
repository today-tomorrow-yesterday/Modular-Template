namespace ModularTemplate.Infrastructure.Outbox.Persistence;

/// <summary>
/// Represents a message in the outbox for reliable event publishing.
/// </summary>
public sealed class OutboxMessage
{
    /// <summary>
    /// Unique identifier of the message.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Type name of the domain event.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Serialized content of the domain event.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// UTC timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredOnUtc { get; init; }

    /// <summary>
    /// UTC timestamp when the message was processed, null if not yet processed.
    /// </summary>
    public DateTime? ProcessedOnUtc { get; set; }

    /// <summary>
    /// Error message if processing failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Number of retry attempts made for this message.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// UTC timestamp when the message should next be retried.
    /// Null means the message is immediately eligible for processing.
    /// </summary>
    public DateTime? NextRetryAtUtc { get; set; }
}
