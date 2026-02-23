using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Sales.Domain;
using Modules.Sales.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Inbox.Job;
using System.Reflection;

namespace Modules.Sales.Infrastructure.Inbox;

internal sealed class ProcessInboxJob(
    IDbConnectionFactory<ISalesModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessInboxJob> logger)
    : ProcessInboxJobBase<ISalesModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, inboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Sales";

    protected override string Schema => Schemas.Sales;

    protected override Assembly HandlersAssembly => Presentation.AssemblyReference.Assembly;
}
