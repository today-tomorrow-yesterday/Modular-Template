using Dapper;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Infrastructure.Inbox.Persistence;
using System.Data.Common;

namespace ModularTemplate.Infrastructure.Inbox.Handlers;

/// <summary>
/// Base decorator that ensures idempotent integration event handling by tracking consumed messages.
/// </summary>
/// <typeparam name="TIntegrationEvent">The type of integration event.</typeparam>
/// <typeparam name="TModule">The module marker interface type.</typeparam>
/// <remarks>
/// <para>
/// This decorator wraps an integration event handler and ensures that the same event is not
/// processed twice by the same handler. It uses a database table to track which handlers
/// have already consumed a given message.
/// </para>
/// <para>
/// Module-specific implementations only need to provide the database schema where the
/// inbox_message_consumers table resides.
/// </para>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="IdempotentIntegrationEventHandlerBase{TIntegrationEvent, TModule}"/> class.
/// </remarks>
/// <param name="decorated">The inner handler to decorate with idempotency.</param>
/// <param name="dbConnectionFactory">Factory for creating database connections.</param>
public abstract class IdempotentIntegrationEventHandlerBase<TIntegrationEvent, TModule>(
    IIntegrationEventHandler<TIntegrationEvent> decorated,
    IDbConnectionFactory<TModule> dbConnectionFactory) : IntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IIntegrationEvent
    where TModule : class
{
    private readonly IIntegrationEventHandler<TIntegrationEvent> _decorated = decorated;
    private readonly IDbConnectionFactory<TModule> _dbConnectionFactory = dbConnectionFactory;

    /// <summary>
    /// Gets the database schema where the inbox_message_consumers table resides.
    /// </summary>
    protected abstract string Schema { get; }

    /// <summary>
    /// Handles the integration event with idempotency checking.
    /// </summary>
    public override async Task HandleAsync(TIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

        var inboxMessageConsumer = new InboxMessageConsumer(integrationEvent.Id, _decorated.GetType().Name);

        // Atomically insert the consumer record; if it already exists, skip handling.
        if (!await TryInsertInboxConsumerAsync(connection, inboxMessageConsumer))
        {
            return;
        }

        await _decorated.HandleAsync(integrationEvent, cancellationToken);
    }

    private static async Task<bool> TryInsertInboxConsumerAsync(
        DbConnection dbConnection,
        InboxMessageConsumer inboxMessageConsumer)
    {
        var sql =
            """
            INSERT INTO messaging.inbox_message_consumers(inbox_message_id, name)
            VALUES (@InboxMessageId, @Name)
            ON CONFLICT DO NOTHING
            """;

        var rowsAffected = await dbConnection.ExecuteAsync(sql, inboxMessageConsumer);
        return rowsAffected > 0;
    }
}
