using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Quartz;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Infrastructure.FeatureManagement;

namespace Rtl.Core.Infrastructure.EventBus.Aws;

/// <summary>
/// Base class for Quartz jobs that poll SQS queues for EventBridge events.
/// </summary>
/// <remarks>
/// <para>
/// This abstract base class implements the SQS polling logic for receiving events
/// from EventBridge via SQS. Events are deserialized from the EventBridge envelope
/// format and dispatched to the appropriate handlers.
/// </para>
/// <para>
/// Successfully processed messages are deleted from the queue. Failed messages are
/// left in the queue and will become visible again after the visibility timeout,
/// allowing for automatic retry. After the maximum receive count, SQS will move
/// messages to the dead-letter queue if configured.
/// </para>
/// <para>
/// Polling can be disabled via the <see cref="InfrastructureFeatures.BackgroundJobs"/> feature flag.
/// When disabled, messages remain in the SQS queue and will be processed when the feature is re-enabled.
/// </para>
/// </remarks>

[DisallowConcurrentExecution]
public abstract class SqsPollingJobBase(
    IAmazonSQS sqsClient,
    IEventDispatcher eventDispatcher,
    IOptions<AwsMessagingOptions> options,
    IFeatureFlagService featureFlagService,
    ILogger logger) : IJob
{
    private readonly IAmazonSQS _sqsClient = sqsClient;
    private readonly IEventDispatcher _eventDispatcher = eventDispatcher;
    private readonly AwsMessagingOptions _options = options.Value;
    private readonly IFeatureFlagService _featureFlagService = featureFlagService;
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Gets the name of the module this job processes messages for.
    /// </summary>
    /// <remarks>Used for logging and identification purposes.</remarks>
    protected abstract string ModuleName { get; }

    /// <summary>
    /// Executes the SQS polling job.
    /// </summary>
    public async Task Execute(IJobExecutionContext context)
    {
        if (!_featureFlagService.IsEnabled(InfrastructureFeatures.BackgroundJobs))
        {
            _logger.LogDebug(
                "{Module} - SQS polling is disabled via feature flag. Messages will remain in queue.",
                ModuleName);
            return;
        }

        _logger.LogDebug("{Module} - Polling SQS queue for messages", ModuleName);

        var request = new ReceiveMessageRequest
        {
            QueueUrl = _options.SqsQueueUrl,
            MaxNumberOfMessages = _options.MaxMessages,
            WaitTimeSeconds = 20, // Long polling
            VisibilityTimeout = _options.VisibilityTimeoutSeconds
        };

        var response = await _sqsClient.ReceiveMessageAsync(request, context.CancellationToken);

        if (response.Messages.Count == 0)
        {
            _logger.LogDebug("{Module} - No messages received from SQS queue", ModuleName);
            return;
        }

        _logger.LogInformation(
            "{Module} - Received {Count} messages from SQS queue",
            ModuleName,
            response.Messages.Count);

        foreach (var message in response.Messages)
        {
            await ProcessMessageAsync(message, context.CancellationToken);
        }

        _logger.LogInformation("{Module} - Completed processing SQS messages", ModuleName);
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "{Module} - Processing message {MessageId}",
                ModuleName,
                message.MessageId);

            var envelope = JsonConvert.DeserializeObject<EventBridgeEnvelope>(message.Body);

            if (envelope is null)
            {
                _logger.LogWarning(
                    "{Module} - Failed to deserialize message {MessageId} as EventBridge envelope",
                    ModuleName,
                    message.MessageId);
                return;
            }

            await _eventDispatcher.DispatchAsync(envelope.DetailType, envelope.Detail, cancellationToken);

            // Successfully processed - delete the message
            await _sqsClient.DeleteMessageAsync(
                _options.SqsQueueUrl,
                message.ReceiptHandle,
                cancellationToken);

            _logger.LogDebug(
                "{Module} - Successfully processed and deleted message {MessageId}",
                ModuleName,
                message.MessageId);
        }
        catch (Exception ex)
        {
            // Log the error but do NOT delete the message
            // It will become visible again after visibility timeout
            // SQS will move to DLQ after max receives
            _logger.LogError(
                ex,
                "{Module} - Error processing message {MessageId}. Message will be retried after visibility timeout.",
                ModuleName,
                message.MessageId);
        }
    }
}
