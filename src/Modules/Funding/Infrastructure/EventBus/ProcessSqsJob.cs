using Amazon.SQS;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rtl.Core.Application.EventBus;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Infrastructure.EventBus.Aws;

namespace Modules.Funding.Infrastructure.EventBus;

internal sealed class ProcessSqsJob(
    IAmazonSQS sqsClient,
    IEventDispatcher eventDispatcher,
    IOptions<SqsConsumerOptions> options,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessSqsJob> logger)
    : SqsPollingJobBase(sqsClient, eventDispatcher, options, featureFlagService, logger)
{
    protected override string ModuleName => "Funding";
}
