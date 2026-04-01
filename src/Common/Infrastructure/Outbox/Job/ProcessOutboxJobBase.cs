using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain;
using ModularTemplate.Domain.Events;
using ModularTemplate.Infrastructure.FeatureManagement;
using ModularTemplate.Infrastructure.Messaging;
using ModularTemplate.Infrastructure.Outbox.Handler;
using ModularTemplate.Infrastructure.Serialization;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ModularTemplate.Infrastructure.Outbox.Job;

/// <summary>
/// Base class for Quartz jobs that process outbox messages and invoke domain event handlers.
/// </summary>
/// <remarks>
/// <para>
/// This abstract base class contains all the generic processing logic for the outbox pattern,
/// including message retrieval, handler invocation, retry logic with exponential backoff,
/// and dead-letter handling.
/// </para>
/// <para>
/// Module-specific implementations only need to provide the module name, database schema,
/// and the assembly containing the domain event handlers.
/// </para>
/// </remarks>
[DisallowConcurrentExecution]
public abstract class ProcessOutboxJobBase<TModule>(
    IDbConnectionFactory<TModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger logger) : IJob
    where TModule : class
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    /// <summary>
    /// Gets the name of the module this job processes messages for.
    /// </summary>
    /// <remarks>Used for logging and identification purposes.</remarks>
    protected abstract string ModuleName { get; }

    /// <summary>
    /// Gets the database schema where the outbox tables reside.
    /// </summary>
    /// <example>"sample", "orders", "users"</example>
    protected abstract string Schema { get; }

    /// <summary>
    /// Gets the assembly containing the domain event handlers to invoke.
    /// </summary>
    protected abstract Assembly HandlersAssembly { get; }

    /// <summary>
    /// Executes the outbox processing job.
    /// </summary>
    /// <remarks>
    /// If the <see cref="InfrastructureFeatures.Outbox"/> feature flag is disabled,
    /// the job will skip processing. Messages remain in the outbox and will be
    /// processed when the feature is re-enabled.
    /// </remarks>
    public async Task Execute(IJobExecutionContext context)
    {
        if (!featureFlagService.IsEnabled(InfrastructureFeatures.Outbox))
        {
            logger.LogWarning(
                "{Module} - Outbox processing is disabled via feature flag. Messages will remain queued.",
                ModuleName);
            return;
        }

        logger.LogInformation("{Module} - Beginning to process outbox messages", ModuleName);

        await using var connection = await dbConnectionFactory.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        var outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

        foreach (var outboxMessage in outboxMessages)
        {
            Exception? exception = null;

            try
            {
                // Check cancellation before potentially long deserialization
                context.CancellationToken.ThrowIfCancellationRequested();

                var domainEventType = Type.GetType(outboxMessage.Type)
                    ?? ResolveTypeByName(outboxMessage.Type);
                var domainEvent = (IDomainEvent)JsonConvert.DeserializeObject(
                    outboxMessage.Content,
                    domainEventType,
                    SerializerSettings.Instance)!;

                using var scope = serviceScopeFactory.CreateScope();

                var handlers = DomainEventHandlersFactory.GetHandlers(
                    domainEvent.GetType(),
                    scope.ServiceProvider,
                    HandlersAssembly);

                foreach (var domainEventHandler in handlers)
                {
                    await domainEventHandler.Handle(domainEvent, context.CancellationToken);
                }
            }
            catch (Exception caughtException)
            {
                logger.LogError(
                    caughtException,
                    "{Module} - Exception while processing outbox message {MessageId}",
                    ModuleName,
                    outboxMessage.Id);

                exception = caughtException;
            }

            await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
        }

        await transaction.CommitAsync();

        logger.LogInformation("{Module} - Completed processing outbox messages", ModuleName);
    }

    private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        var sql =
            $"""
             SELECT
                id AS {nameof(OutboxMessageResponse.Id)},
                type AS {nameof(OutboxMessageResponse.Type)},
                content AS {nameof(OutboxMessageResponse.Content)},
                retry_count AS {nameof(OutboxMessageResponse.RetryCount)}
             FROM messaging.outbox_messages
             WHERE processed_on_utc IS NULL
               AND (next_retry_at_utc IS NULL OR next_retry_at_utc <= @Now)
             ORDER BY occurred_on_utc
             LIMIT {_outboxOptions.BatchSize}
             FOR UPDATE
             """;

        var outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(
            sql,
            new { Now = dateTimeProvider.UtcNow },
            transaction: transaction);

        return outboxMessages.ToList();
    }

    private async Task UpdateOutboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        OutboxMessageResponse outboxMessage,
        Exception? exception)
    {
        var now = dateTimeProvider.UtcNow;

        if (exception is null)
        {
            // Success: mark as processed
            var successSql =
                $"""
                UPDATE messaging.outbox_messages
                SET processed_on_utc = @ProcessedOnUtc,
                    error = NULL
                WHERE id = @Id
                """;

            await connection.ExecuteAsync(
                successSql,
                new { outboxMessage.Id, ProcessedOnUtc = now },
                transaction: transaction);
        }
        else
        {
            var newRetryCount = outboxMessage.RetryCount + 1;

            if (newRetryCount >= _outboxOptions.MaxRetries)
            {
                // Dead letter: mark as processed with error
                logger.LogError(
                    "Message {MessageId} moved to dead letter after {Retries} retries",
                    outboxMessage.Id,
                    newRetryCount);

                var deadLetterSql =
                    $"""
                    UPDATE messaging.outbox_messages
                    SET processed_on_utc = @ProcessedOnUtc,
                        error = @Error,
                        retry_count = @RetryCount
                    WHERE id = @Id
                    """;

                await connection.ExecuteAsync(
                    deadLetterSql,
                    new
                    {
                        outboxMessage.Id,
                        ProcessedOnUtc = now,
                        Error = exception.ToString(),
                        RetryCount = newRetryCount
                    },
                    transaction: transaction);
            }
            else
            {
                // Schedule retry with exponential backoff
                var nextRetryAt = RetryPolicy.CalculateNextRetry(newRetryCount, now);

                logger.LogWarning(
                    "Message {MessageId} scheduled for retry {Retry}/{Max} at {NextRetry}",
                    outboxMessage.Id,
                    newRetryCount,
                    _outboxOptions.MaxRetries,
                    nextRetryAt);

                var retrySql =
                    $"""
                    UPDATE messaging.outbox_messages
                    SET error = @Error,
                        retry_count = @RetryCount,
                        next_retry_at_utc = @NextRetryAtUtc
                    WHERE id = @Id
                    """;

                await connection.ExecuteAsync(
                    retrySql,
                    new
                    {
                        outboxMessage.Id,
                        Error = exception.ToString(),
                        RetryCount = newRetryCount,
                        NextRetryAtUtc = nextRetryAt
                    },
                    transaction: transaction);
            }
        }
    }

    /// <summary>
    /// Resolves a domain event type by short name when Type.GetType() fails.
    /// The outbox stores short class names (e.g., "CustomerCreatedDomainEvent").
    /// This scans HandlersAssembly and its referenced assemblies (which includes
    /// the Domain assembly where events are defined).
    /// </summary>
    private Type ResolveTypeByName(string typeName)
    {
        var assemblies = new[] { HandlersAssembly }
            .Concat(HandlersAssembly.GetReferencedAssemblies().Select(Assembly.Load));

        foreach (var assembly in assemblies)
        {
            var type = assembly.GetTypes().FirstOrDefault(t => t.Name == typeName);
            if (type is not null) return type;
        }

        throw new InvalidOperationException(
            $"Could not resolve domain event type '{typeName}' in {HandlersAssembly.GetName().Name} or its referenced assemblies.");
    }

    /// <summary>
    /// Response model for outbox message queries.
    /// </summary>
    protected internal sealed record OutboxMessageResponse(Guid Id, string Type, string Content, int RetryCount);
}
