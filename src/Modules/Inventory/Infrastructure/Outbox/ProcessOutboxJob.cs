using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Inventory.Domain;
using Modules.Inventory.Infrastructure.Persistence;
using Rtl.Core.Application.FeatureManagement;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Domain;
using Rtl.Core.Infrastructure.Outbox.Job;
using System.Reflection;

namespace Modules.Inventory.Infrastructure.Outbox;

internal sealed class ProcessOutboxJob(
    IDbConnectionFactory<IInventoryModule> dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    IFeatureFlagService featureFlagService,
    ILogger<ProcessOutboxJob> logger)
    : ProcessOutboxJobBase<IInventoryModule>(dbConnectionFactory, serviceScopeFactory, dateTimeProvider, outboxOptions, featureFlagService, logger)
{
    protected override string ModuleName => "Inventory";

    protected override string Schema => Schemas.Inventories;

    protected override Assembly HandlersAssembly => Application.AssemblyReference.Assembly;
}
