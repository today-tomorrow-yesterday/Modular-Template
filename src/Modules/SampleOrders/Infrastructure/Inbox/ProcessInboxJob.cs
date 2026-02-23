using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.SampleOrders.Domain;
using Modules.SampleOrders.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Inbox.Job;
using System.Reflection;

namespace Modules.SampleOrders.Infrastructure.Inbox;

internal sealed class ProcessInboxJob(
    IDbConnectionFactory<ISampleOrdersModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessInboxJob> logger)
    : ProcessInboxJobBase<ISampleOrdersModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, inboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "SampleOrders";

    protected override string Schema => Schemas.Orders;

    protected override Assembly HandlersAssembly => Presentation.AssemblyReference.Assembly;
}
