using Microsoft.Extensions.DependencyInjection;
using Modules.Customer.Integration.Shared;
using Npgsql;
using Quartz;
using Rtl.Core.Application.EventBus;

namespace Modules.Customer.EventProducerTests.Abstractions;

// Test fixture for verifying that Customer commands produce the correct integration events.
// Uses SpyEventBus to capture events instead of dispatching them to consumers.
//
// Overrides GetSchemasToInclude to also clean the messaging schema (outbox) between tests,
// preventing stale outbox messages from prior runs from being re-processed.
public class EventProducerTestFixture : CustomerTestFixtureBase
{
    public SpyEventBus Spy { get; } = new();

    protected override string[] GetSchemasToInclude()
        => ["customers", "messaging"];

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        // Replace the event bus with our spy (singleton so the same instance is resolved in every scope)
        services.AddSingleton<IEventBus>(Spy);
    }

    // Clear captured events on each reset
    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        Spy.Clear();
    }

    // Flush the Customer outbox -- triggers domain event handlers which publish to SpyEventBus.
    // Same approach as Sales EventConsumerTests: trigger the Quartz job + poll outbox table.
    public async Task FlushOutboxAsync()
    {
        var schedulerFactory = Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob");
        await scheduler.TriggerJob(jobKey);

        // Poll until the outbox is empty (max 5 seconds)
        await using var conn = new NpgsqlConnection(ResolvedConnectionString);
        await conn.OpenAsync();
        for (var i = 0; i < 50; i++)
        {
            await Task.Delay(100);
            await using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM messaging.outbox_messages WHERE processed_on_utc IS NULL", conn);
            var pending = (long)(await cmd.ExecuteScalarAsync())!;
            if (pending == 0) return;
        }
    }
}
