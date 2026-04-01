namespace ModularTemplate.Infrastructure.Inbox.Persistence;

/// <summary>
/// Tracks which handlers have consumed an inbox message for idempotency.
/// </summary>
public sealed class InboxMessageConsumer(Guid inboxMessageId, string name)
{
    public Guid InboxMessageId { get; init; } = inboxMessageId;

    public string Name { get; init; } = name;
}
