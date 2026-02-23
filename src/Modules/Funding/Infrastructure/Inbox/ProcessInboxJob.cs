using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Funding.Domain;
using Modules.Funding.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Inbox.Job;
using System.Reflection;

namespace Modules.Funding.Infrastructure.Inbox;

internal sealed class ProcessInboxJob(
    IDbConnectionFactory<IFundingModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessInboxJob> logger)
    : ProcessInboxJobBase<IFundingModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, inboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Funding";

    protected override string Schema => Schemas.Fundings;

    protected override Assembly HandlersAssembly => Presentation.AssemblyReference.Assembly;
}
