namespace ModularTemplate.Infrastructure.Outbox.Persistence;

/// <summary>
/// Tracks which handlers have consumed an outbox message for idempotency.
/// </summary>
public sealed class OutboxMessageConsumer(Guid outboxMessageId, string name)
{
    public Guid OutboxMessageId { get; init; } = outboxMessageId;

    public string Name { get; init; } = name;
}
