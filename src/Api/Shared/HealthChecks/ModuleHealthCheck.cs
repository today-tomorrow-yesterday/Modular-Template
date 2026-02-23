using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Api.Shared.HealthChecks;

/// <summary>
/// Configuration for a module health check.
/// </summary>
public sealed class ModuleHealthCheckOptions : IValidatableObject
{
    /// <summary>
    /// Gets or sets the module name.
    /// </summary>
    public required string ModuleName { get; set; }

    /// <summary>
    /// Gets or sets the database schema for this module.
    /// </summary>
    public required string Schema { get; set; }

    /// <summary>
    /// Gets or sets the age threshold in seconds for pending outbox messages to indicate degraded health.
    /// </summary>
    public int OutboxDegradedThresholdSeconds { get; set; }

    /// <summary>
    /// Gets or sets the age threshold in seconds for pending outbox messages to indicate unhealthy status.
    /// </summary>
    public int OutboxUnhealthyThresholdSeconds { get; set; }

    /// <summary>
    /// Gets or sets the count threshold for pending outbox messages to indicate degraded health.
    /// </summary>
    public int OutboxDegradedCountThreshold { get; set; }

    /// <summary>
    /// Gets or sets the count threshold for pending outbox messages to indicate unhealthy status.
    /// </summary>
    public int OutboxUnhealthyCountThreshold { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ModuleName))
        {
            yield return new ValidationResult(
                "ModuleName is required.",
                [nameof(ModuleName)]);
        }

        if (string.IsNullOrWhiteSpace(Schema))
        {
            yield return new ValidationResult(
                "Schema is required.",
                [nameof(Schema)]);
        }

        if (OutboxDegradedThresholdSeconds <= 0)
        {
            yield return new ValidationResult(
                "OutboxDegradedThresholdSeconds must be positive.",
                [nameof(OutboxDegradedThresholdSeconds)]);
        }

        if (OutboxUnhealthyThresholdSeconds <= 0)
        {
            yield return new ValidationResult(
                "OutboxUnhealthyThresholdSeconds must be positive.",
                [nameof(OutboxUnhealthyThresholdSeconds)]);
        }

        if (OutboxUnhealthyThresholdSeconds <= OutboxDegradedThresholdSeconds)
        {
            yield return new ValidationResult(
                "OutboxUnhealthyThresholdSeconds must be greater than OutboxDegradedThresholdSeconds.",
                [nameof(OutboxUnhealthyThresholdSeconds), nameof(OutboxDegradedThresholdSeconds)]);
        }

        if (OutboxDegradedCountThreshold <= 0)
        {
            yield return new ValidationResult(
                "OutboxDegradedCountThreshold must be positive.",
                [nameof(OutboxDegradedCountThreshold)]);
        }

        if (OutboxUnhealthyCountThreshold <= 0)
        {
            yield return new ValidationResult(
                "OutboxUnhealthyCountThreshold must be positive.",
                [nameof(OutboxUnhealthyCountThreshold)]);
        }

        if (OutboxUnhealthyCountThreshold <= OutboxDegradedCountThreshold)
        {
            yield return new ValidationResult(
                "OutboxUnhealthyCountThreshold must be greater than OutboxDegradedCountThreshold.",
                [nameof(OutboxUnhealthyCountThreshold), nameof(OutboxDegradedCountThreshold)]);
        }
    }
}

/// <summary>
/// Health check for an individual module that monitors its outbox and inbox queues.
/// </summary>
public sealed class ModuleHealthCheck(
    NpgsqlDataSource dataSource,
    ModuleHealthCheckOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var data = new Dictionary<string, object>
            {
                ["module"] = options.ModuleName,
                ["schema"] = options.Schema
            };

            // Check outbox status
            var outboxStatus = await CheckOutboxAsync(cancellationToken);
            data["outbox"] = new
            {
                pendingCount = outboxStatus.PendingCount,
                oldestPendingAgeSeconds = outboxStatus.OldestPendingAgeSeconds,
                failedCount = outboxStatus.FailedCount
            };

            // Check inbox status
            var inboxStatus = await CheckInboxAsync(cancellationToken);
            data["inbox"] = new
            {
                pendingCount = inboxStatus.PendingCount,
                oldestPendingAgeSeconds = inboxStatus.OldestPendingAgeSeconds,
                failedCount = inboxStatus.FailedCount
            };

            // Determine overall status
            var outboxHealth = DetermineOutboxStatus(outboxStatus);
            var inboxHealth = DetermineOutboxStatus(inboxStatus); // Using same thresholds for inbox

            var overallStatus = (outboxHealth, inboxHealth) switch
            {
                (HealthStatus.Unhealthy, _) => HealthStatus.Unhealthy,
                (_, HealthStatus.Unhealthy) => HealthStatus.Unhealthy,
                (HealthStatus.Degraded, _) => HealthStatus.Degraded,
                (_, HealthStatus.Degraded) => HealthStatus.Degraded,
                _ => HealthStatus.Healthy
            };

            data["outboxStatus"] = outboxHealth.ToString();
            data["inboxStatus"] = inboxHealth.ToString();

            var description = overallStatus switch
            {
                HealthStatus.Healthy => $"Module {options.ModuleName} healthy",
                HealthStatus.Degraded => $"Module {options.ModuleName} degraded: outbox={outboxStatus.PendingCount} pending, inbox={inboxStatus.PendingCount} pending",
                _ => $"Module {options.ModuleName} unhealthy: outbox={outboxStatus.PendingCount} pending/{outboxStatus.FailedCount} failed, inbox={inboxStatus.PendingCount} pending/{inboxStatus.FailedCount} failed"
            };

            return new HealthCheckResult(overallStatus, description, data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Failed to check module {options.ModuleName} health", ex);
        }
    }

    private async Task<MessageQueueStatus> CheckOutboxAsync(CancellationToken cancellationToken)
    {
        return await CheckMessageTableAsync("outbox_messages", cancellationToken);
    }

    private async Task<MessageQueueStatus> CheckInboxAsync(CancellationToken cancellationToken)
    {
        return await CheckMessageTableAsync("inbox_messages", cancellationToken);
    }

    private async Task<MessageQueueStatus> CheckMessageTableAsync(string tableName, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        var query = $"""
            SELECT
                COUNT(*) FILTER (WHERE processed_on_utc IS NULL) as pending_count,
                COALESCE(EXTRACT(EPOCH FROM (NOW() - MIN(occurred_on_utc))) FILTER (WHERE processed_on_utc IS NULL), 0) as oldest_age_seconds,
                COUNT(*) FILTER (WHERE processed_on_utc IS NULL AND error IS NOT NULL) as failed_count
            FROM {options.Schema}.{tableName}
            WHERE processed_on_utc IS NULL
               OR (processed_on_utc IS NULL AND occurred_on_utc > NOW() - INTERVAL '1 day')
            """;

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            return new MessageQueueStatus(
                reader.GetInt64(0),
                reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                reader.GetInt64(2));
        }

        return new MessageQueueStatus(0, 0, 0);
    }

    private HealthStatus DetermineOutboxStatus(MessageQueueStatus status)
    {
        if (status.OldestPendingAgeSeconds >= options.OutboxUnhealthyThresholdSeconds ||
            status.PendingCount >= options.OutboxUnhealthyCountThreshold)
        {
            return HealthStatus.Unhealthy;
        }

        if (status.OldestPendingAgeSeconds >= options.OutboxDegradedThresholdSeconds ||
            status.PendingCount >= options.OutboxDegradedCountThreshold)
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    private sealed record MessageQueueStatus(
        long PendingCount,
        double OldestPendingAgeSeconds,
        long FailedCount);
}
