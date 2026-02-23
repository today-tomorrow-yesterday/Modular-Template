using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Inbox.Job;
using System.Reflection;

namespace Modules.SampleSales.Infrastructure.Inbox;

internal sealed class ProcessInboxJob(
    IDbConnectionFactory<ISampleSalesModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessInboxJob> logger)
    : ProcessInboxJobBase<ISampleSalesModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, inboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "SampleSales";

    protected override string Schema => Schemas.Sample;

    protected override Assembly HandlersAssembly => Presentation.AssemblyReference.Assembly;
}
