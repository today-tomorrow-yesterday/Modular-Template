using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.SampleOrders.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<ISampleOrdersModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "SampleOrders";

    protected override string Schema => Schemas.Orders;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
