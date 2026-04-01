using Dapper;
using ModularTemplate.Application.Messaging;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain.Events;
using ModularTemplate.Infrastructure.Outbox.Persistence;
using System.Data.Common;

namespace ModularTemplate.Infrastructure.Outbox.Handler;

/// <summary>
/// Base decorator that ensures idempotent domain event handling by tracking consumed messages.
/// </summary>
public abstract class IdempotentDomainEventHandlerBase<TDomainEvent, TModule>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory<TModule> dbConnectionFactory) : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
    where TModule : class
{
    private readonly IDomainEventHandler<TDomainEvent> _decorated = decorated;
    private readonly IDbConnectionFactory<TModule> _dbConnectionFactory = dbConnectionFactory;

    /// <summary>
    /// Gets the database schema where the outbox_message_consumers table resides.
    /// </summary>
    protected abstract string Schema { get; }

    public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.OpenConnectionAsync();

        var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, _decorated.GetType().Name);

        if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer))
        {
            return;
        }

        await _decorated.Handle(domainEvent, cancellationToken);

        await InsertOutboxConsumerAsync(connection, outboxMessageConsumer);
    }

    private async Task<bool> OutboxConsumerExistsAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        var sql =
            $"""
            SELECT EXISTS(
                SELECT 1
                FROM messaging.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId AND
                      name = @Name
            )
            """;

        return await dbConnection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
    }

    private async Task InsertOutboxConsumerAsync(
        DbConnection dbConnection,
        OutboxMessageConsumer outboxMessageConsumer)
    {
        var sql =
            $"""
            INSERT INTO messaging.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;

        await dbConnection.ExecuteAsync(sql, outboxMessageConsumer);
    }
}
