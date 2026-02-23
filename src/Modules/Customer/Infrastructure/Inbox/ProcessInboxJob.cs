using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Customer.Domain;
using Modules.Customer.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Inbox.Job;
using System.Reflection;

namespace Modules.Customer.Infrastructure.Inbox;

internal sealed class ProcessInboxJob(
    IDbConnectionFactory<ICustomerModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessInboxJob> logger)
    : ProcessInboxJobBase<ICustomerModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, inboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Customer";

    protected override string Schema => Schemas.Customers;

    protected override Assembly HandlersAssembly => Presentation.AssemblyReference.Assembly;
}
