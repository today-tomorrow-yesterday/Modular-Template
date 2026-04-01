using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.SampleSales.Domain;
using Modules.SampleSales.Infrastructure.Persistence;
using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Application.Persistence;
using ModularTemplate.Domain;
using ModularTemplate.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.SampleSales.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<ISampleSalesModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<ISampleSalesModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "SampleSales";

    protected override string Schema => Schemas.Sample;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
