using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Infrastructure.Persistence;
using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain;
using ModularTemplate.Infrastructure.Inbox.Job;
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
