using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Customer.Domain;
using Modules.Customer.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.Customer.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<ICustomerModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<ICustomerModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Customer";

    protected override string Schema => Schemas.Customers;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
