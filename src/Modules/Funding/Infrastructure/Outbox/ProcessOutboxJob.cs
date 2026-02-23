using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Funding.Domain;
using Modules.Funding.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.Funding.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<IFundingModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<IFundingModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Funding";

    protected override string Schema => Schemas.Fundings;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
