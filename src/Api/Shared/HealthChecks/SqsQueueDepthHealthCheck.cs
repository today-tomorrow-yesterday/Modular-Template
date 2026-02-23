using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Api.Shared.HealthChecks;

/// <summary>
/// Configuration options for the SQS queue depth health check.
/// </summary>
public sealed class SqsQueueDepthHealthCheckOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "HealthChecks:SqsQueueDepth";

    /// <summary>
    /// Gets or sets the SQS queue URL to check.
    /// </summary>
    public string QueueUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the message count threshold for degraded health.
    /// </summary>
    public int DegradedThreshold { get; set; }

    /// <summary>
    /// Gets or sets the message count threshold for unhealthy status.
    /// </summary>
    public int UnhealthyThreshold { get; set; }

    /// <summary>
    /// Gets or sets whether to include dead letter queue metrics if available.
    /// </summary>
    public bool IncludeDeadLetterQueue { get; set; }

    /// <summary>
    /// Gets or sets the dead letter queue URL to check.
    /// </summary>
    public string? DeadLetterQueueUrl { get; set; }

    /// <summary>
    /// Gets or sets the threshold for messages in the dead letter queue that indicates unhealthy status.
    /// </summary>
    public int DeadLetterQueueUnhealthyThreshold { get; set; }

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Only validate thresholds if SQS is configured (QueueUrl is set)
        if (!string.IsNullOrWhiteSpace(QueueUrl))
        {
            if (DegradedThreshold <= 0)
            {
                yield return new ValidationResult(
                    "DegradedThreshold must be positive when SQS is configured.",
                    [nameof(DegradedThreshold)]);
            }

            if (UnhealthyThreshold <= 0)
            {
                yield return new ValidationResult(
                    "UnhealthyThreshold must be positive when SQS is configured.",
                    [nameof(UnhealthyThreshold)]);
            }

            if (UnhealthyThreshold <= DegradedThreshold)
            {
                yield return new ValidationResult(
                    "UnhealthyThreshold must be greater than DegradedThreshold.",
                    [nameof(UnhealthyThreshold), nameof(DegradedThreshold)]);
            }

            if (IncludeDeadLetterQueue && DeadLetterQueueUnhealthyThreshold < 0)
            {
                yield return new ValidationResult(
                    "DeadLetterQueueUnhealthyThreshold cannot be negative.",
                    [nameof(DeadLetterQueueUnhealthyThreshold)]);
            }
        }
    }
}

/// <summary>
/// Health check that monitors SQS queue depth and dead letter queue status.
/// </summary>
public sealed class SqsQueueDepthHealthCheck(
    IAmazonSQS? sqsClient,
    IOptions<SqsQueueDepthHealthCheckOptions> options,
    ILogger<SqsQueueDepthHealthCheck> logger) : IHealthCheck
{
    private readonly SqsQueueDepthHealthCheckOptions _options = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // If SQS is not configured, skip this check gracefully
        if (sqsClient is null || string.IsNullOrEmpty(_options.QueueUrl))
        {
            return HealthCheckResult.Healthy(
                "SQS queue depth check skipped: AWS SQS is not configured",
                new Dictionary<string, object> { ["skipped"] = true });
        }

        try
        {
            Dictionary<string, object> data = [];

            // Get main queue attributes
            var mainQueueMetrics = await GetQueueMetricsAsync(_options.QueueUrl, cancellationToken);
            data["mainQueue"] = new
            {
                url = _options.QueueUrl,
                approximateMessageCount = mainQueueMetrics.ApproximateMessageCount,
                approximateNotVisibleCount = mainQueueMetrics.ApproximateNotVisibleCount,
                approximateDelayedCount = mainQueueMetrics.ApproximateDelayedCount
            };

            var status = DetermineMainQueueStatus(mainQueueMetrics);

            // Check dead letter queue if configured
            if (_options.IncludeDeadLetterQueue && !string.IsNullOrEmpty(_options.DeadLetterQueueUrl))
            {
                var dlqMetrics = await GetQueueMetricsAsync(_options.DeadLetterQueueUrl, cancellationToken);
                data["deadLetterQueue"] = new
                {
                    url = _options.DeadLetterQueueUrl,
                    approximateMessageCount = dlqMetrics.ApproximateMessageCount
                };

                if (dlqMetrics.ApproximateMessageCount >= _options.DeadLetterQueueUnhealthyThreshold)
                {
                    status = HealthStatus.Unhealthy;
                    data["deadLetterQueueAlert"] = true;
                }
            }

            var totalMessages = mainQueueMetrics.ApproximateMessageCount +
                               mainQueueMetrics.ApproximateNotVisibleCount +
                               mainQueueMetrics.ApproximateDelayedCount;

            data["totalMessagesInFlight"] = totalMessages;

            var description = status switch
            {
                HealthStatus.Healthy => $"SQS queue healthy: {totalMessages} messages in flight",
                HealthStatus.Degraded => $"SQS queue degraded: {totalMessages} messages in flight (threshold: {_options.DegradedThreshold})",
                _ => $"SQS queue unhealthy: {totalMessages} messages in flight (threshold: {_options.UnhealthyThreshold})"
            };

            return new HealthCheckResult(status, description, data: data);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to check SQS queue depth for {QueueUrl}", _options.QueueUrl);
            return HealthCheckResult.Unhealthy("Failed to check SQS queue depth", ex);
        }
    }

    private async Task<QueueMetrics> GetQueueMetricsAsync(string queueUrl, CancellationToken cancellationToken)
    {
        var request = new GetQueueAttributesRequest
        {
            QueueUrl = queueUrl,
            AttributeNames =
            [
                "ApproximateNumberOfMessages",
                "ApproximateNumberOfMessagesNotVisible",
                "ApproximateNumberOfMessagesDelayed"
            ]
        };

        var response = await sqsClient!.GetQueueAttributesAsync(request, cancellationToken);

        return new QueueMetrics(
            GetAttributeValue(response, "ApproximateNumberOfMessages"),
            GetAttributeValue(response, "ApproximateNumberOfMessagesNotVisible"),
            GetAttributeValue(response, "ApproximateNumberOfMessagesDelayed"));
    }

    private static int GetAttributeValue(GetQueueAttributesResponse response, string attributeName)
    {
        if (response.Attributes.TryGetValue(attributeName, out var value) &&
            int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        return 0;
    }

    private HealthStatus DetermineMainQueueStatus(QueueMetrics metrics)
    {
        var totalMessages = metrics.ApproximateMessageCount +
                           metrics.ApproximateNotVisibleCount +
                           metrics.ApproximateDelayedCount;

        if (totalMessages >= _options.UnhealthyThreshold)
        {
            return HealthStatus.Unhealthy;
        }

        if (totalMessages >= _options.DegradedThreshold)
        {
            return HealthStatus.Degraded;
        }

        return HealthStatus.Healthy;
    }

    private sealed record QueueMetrics(
        int ApproximateMessageCount,
        int ApproximateNotVisibleCount,
        int ApproximateDelayedCount);
}
