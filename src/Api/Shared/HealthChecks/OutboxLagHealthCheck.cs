using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Npgsql;
using System.ComponentModel.DataAnnotations;

namespace ModularTemplate.Api.Shared.HealthChecks;

/// <summary>
/// Configuration options for the outbox lag health check.
/// </summary>
public sealed class OutboxLagHealthCheckOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "HealthChecks:OutboxLag";

    /// <summary>
    /// Gets or sets the database schemas to check for outbox messages.
    /// </summary>
    public string[] Schemas { get; set; } = [];

    /// <summary>
    /// Gets or sets the age threshold in seconds after which pending messages indicate degraded health.
    /// </summary>
    public int DegradedThresholdSeconds { get; set; }

    /// <summary>
    /// Gets or sets the age threshold in seconds after which pending messages indicate unhealthy status.
    /// </summary>
    public int UnhealthyThresholdSeconds { get; set; }

    /// <summary>
    /// Gets or sets the count threshold after which pending messages indicate degraded health.
    /// </summary>
    public int DegradedCountThreshold { get; set; }

    /// <summary>
    /// Gets or sets the count threshold after which pending messages indicate unhealthy status.
    /// </summary>
    public int UnhealthyCountThreshold { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Schemas are injected from ApplicationOptions via PostConfigure
        if (Schemas.Length == 0)
        {
            yield return new ValidationResult(
                "Schemas must be configured via Application:Modules or derived from Application:DatabaseName.",
                [nameof(Schemas)]);
        }

        if (DegradedThresholdSeconds <= 0)
        {
            yield return new ValidationResult(
                "DegradedThresholdSeconds must be positive.",
                [nameof(DegradedThresholdSeconds)]);
        }

        if (UnhealthyThresholdSeconds <= 0)
        {
            yield return new ValidationResult(
                "UnhealthyThresholdSeconds must be positive.",
                [nameof(UnhealthyThresholdSeconds)]);
        }

        if (UnhealthyThresholdSeconds <= DegradedThresholdSeconds)
        {
            yield return new ValidationResult(
                "UnhealthyThresholdSeconds must be greater than DegradedThresholdSeconds.",
                [nameof(UnhealthyThresholdSeconds), nameof(DegradedThresholdSeconds)]);
        }

        if (DegradedCountThreshold <= 0)
        {
            yield return new ValidationResult(
                "DegradedCountThreshold must be positive.",
                [nameof(DegradedCountThreshold)]);
        }

        if (UnhealthyCountThreshold <= 0)
        {
            yield return new ValidationResult(
                "UnhealthyCountThreshold must be positive.",
                [nameof(UnhealthyCountThreshold)]);
        }

        if (UnhealthyCountThreshold <= DegradedCountThreshold)
        {
            yield return new ValidationResult(
                "UnhealthyCountThreshold must be greater than DegradedCountThreshold.",
                [nameof(UnhealthyCountThreshold), nameof(DegradedCountThreshold)]);
        }
    }
}

/// <summary>
/// Health check that monitors the outbox message processing lag.
/// Checks for pending messages that are older than configurable thresholds.
/// </summary>
public sealed class OutboxLagHealthCheck(
    NpgsqlDataSource dataSource,
    IOptions<OutboxLagHealthCheckOptions> options) : IHealthCheck
{
    private readonly OutboxLagHealthCheckOptions _options = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<string, OutboxSchemaStatus> schemaResults = [];
            var overallStatus = HealthStatus.Healthy;

            foreach (var schema in _options.Schemas)
            {
                var status = await CheckSchemaOutboxAsync(schema, cancellationToken);
                schemaResults[schema] = status;

                if (status.Status == HealthStatus.Unhealthy)
                {
                    overallStatus = HealthStatus.Unhealthy;
                }
                else if (status.Status == HealthStatus.Degraded && overallStatus != HealthStatus.Unhealthy)
                {
                    overallStatus = HealthStatus.Degraded;
                }
            }

            var data = new Dictionary<string, object>
            {
                ["schemas"] = schemaResults.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new
                    {
                        pendingCount = kvp.Value.PendingCount,
                        oldestPendingAgeSeconds = kvp.Value.OldestPendingAgeSeconds,
                        failedCount = kvp.Value.FailedCount,
                        status = kvp.Value.Status.ToString()
                    } as object)
            };

            var totalPending = schemaResults.Values.Sum(s => s.PendingCount);
            var totalFailed = schemaResults.Values.Sum(s => s.FailedCount);
            var maxAge = schemaResults.Values.Max(s => s.OldestPendingAgeSeconds);

            data["totalPendingCount"] = totalPending;
            data["totalFailedCount"] = totalFailed;
            data["maxOldestPendingAgeSeconds"] = maxAge;

            var description = overallStatus switch
            {
                HealthStatus.Healthy => $"Outbox healthy: {totalPending} pending messages",
                HealthStatus.Degraded => $"Outbox degraded: {totalPending} pending, oldest {maxAge:F0}s, {totalFailed} failed",
                _ => $"Outbox unhealthy: {totalPending} pending, oldest {maxAge:F0}s, {totalFailed} failed"
            };

            return new HealthCheckResult(overallStatus, description, data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Failed to check outbox lag", ex);
        }
    }

    private async Task<OutboxSchemaStatus> CheckSchemaOutboxAsync(string schema, CancellationToken cancellationToken)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);

        // Query for pending messages count and oldest age
        var query = $"""
            SELECT
                COUNT(*) FILTER (WHERE processed_on_utc IS NULL) as pending_count,
                COALESCE(EXTRACT(EPOCH FROM (NOW() - MIN(occurred_on_utc))) FILTER (WHERE processed_on_utc IS NULL), 0) as oldest_age_seconds,
                COUNT(*) FILTER (WHERE processed_on_utc IS NULL AND error IS NOT NULL) as failed_count
            FROM {schema}.outbox_messages
            WHERE processed_on_utc IS NULL
               OR (processed_on_utc IS NULL AND occurred_on_utc > NOW() - INTERVAL '1 day')
            """;

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        long pendingCount = 0;
        double oldestAgeSeconds = 0;
        long failedCount = 0;

        if (await reader.ReadAsync(cancellationToken))
        {
            pendingCount = reader.GetInt64(0);
            oldestAgeSeconds = reader.IsDBNull(1) ? 0 : reader.GetDouble(1);
            failedCount = reader.GetInt64(2);
        }

        var status = DetermineStatus(pendingCount, oldestAgeSeconds);

        return new OutboxSchemaStatus(pendingCount, oldestAgeSeconds, failedCount, status);
    }

    private HealthStatus DetermineStatus(long pendingCount, double oldestAgeSeconds)
    {
        if (oldestAgeSeconds >= _options.UnhealthyThresholdSeconds ||
            pendingCount >= _options.UnhealthyCountThreshold)
        {
            return HealthStatus.Unhealthy;
        }

        if (oldestAgeSeconds >= _options.DegradedThresholdSeconds ||
            pendingCount >= _options.DegradedCountThreshold)
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    private sealed record OutboxSchemaStatus(
        long PendingCount,
        double OldestPendingAgeSeconds,
        long FailedCount,
        HealthStatus Status);
}
