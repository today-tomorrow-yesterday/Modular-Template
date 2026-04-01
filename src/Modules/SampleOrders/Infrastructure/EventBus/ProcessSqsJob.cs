using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularTemplate.Application.EventBus;
using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Infrastructure.EventBus.Aws;

namespace Modules.SampleOrders.Infrastructure.EventBus;

internal sealed class ProcessSqsJob(
    IAmazonSQS sqsClient,
    IEventDispatcher eventDispatcher,
    IOptions<SqsConsumerOptions> options,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessSqsJob> logger)
    : SqsPollingJobBase(sqsClient, eventDispatcher, options, featureFlagService, logger)
{
    protected override string ModuleName => "SampleOrders";
}
