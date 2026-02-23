using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Organization.Domain;
using Modules.Organization.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.Organization.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<IOrganizationModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<IOrganizationModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Organization";

    protected override string Schema => Schemas.Organizations;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
