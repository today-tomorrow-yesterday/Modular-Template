---
name: scaffold-event-flow-test
description: Use when adding event producer or event consumer integration tests - covers SpyEventBus for producer tests and cross-module outbox flushing for consumer tests
---

# Scaffold Event Flow Tests

Two patterns: **Producer tests** verify a command raises the correct integration event. **Consumer tests** verify an integration event from another module updates the local cache/state correctly.

## Event Producer Tests

Test that a command → domain event → integration event pipeline produces the correct event payload.

### Producer Fixture

```csharp
public class EventProducerTestFixture : {Module}TestFixtureBase
{
    public SpyEventBus Spy { get; } = new();

    protected override string[] GetSchemasToInclude()
        => ["{module_schema}", "messaging"];  // Must include messaging to clear outbox

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.AddSingleton<IEventBus>(Spy);  // Replace real bus with spy
    }

    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        Spy.Clear();
    }

    public async Task FlushOutboxAsync()
    {
        var schedulerFactory = Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("Modules.{Module}.Infrastructure.Outbox.ProcessOutboxJob");
        await scheduler.TriggerJob(jobKey);

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
```

### SpyEventBus

```csharp
public sealed class SpyEventBus : IEventBus
{
    private readonly List<IIntegrationEvent> _events = [];
    public IReadOnlyList<IIntegrationEvent> PublishedEvents => _events;

    public Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        _events.Add(integrationEvent);
        return Task.CompletedTask;
    }

    public void Clear() => _events.Clear();
    public T GetSingle<T>() where T : IIntegrationEvent => (T)_events.Single(e => e is T);
    public IEnumerable<T> GetAll<T>() where T : IIntegrationEvent => _events.OfType<T>();
    public bool HasEvent<T>() where T : IIntegrationEvent => _events.Any(e => e is T);
}
```

### Producer Test Pattern

```csharp
[Fact]
public async Task {Command}_Produces{Event}_WithCorrectPayload()
{
    // Arrange — send command
    using var scope = Fixture.Services.CreateScope();
    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
    var command = new {Command}(...);
    var result = await sender.Send(command);
    Assert.True(result.IsSuccess);

    // Act — flush outbox
    await Fixture.FlushOutboxAsync();

    // Assert — SpyEventBus captured the event
    Assert.True(Spy.HasEvent<{IntegrationEvent}>());
    var evt = Spy.GetSingle<{IntegrationEvent}>();
    Assert.Equal("ExpectedValue", evt.SomeProperty);
    Assert.NotEqual(Guid.Empty, evt.Public{Entity}Id);
}
```

**For update tests:** Create → flush → `Spy.Clear()` → update → flush → assert only update event.

## Event Consumer Tests

Test that an integration event from Module A correctly updates Module B's cache/state.

### Consumer Fixture (Cross-Module)

```csharp
public class EventConsumerTestFixture : {ConsumerModule}TestFixtureBase
{
    private Respawner? _sourceRespawner;

    public override async Task ResetDatabaseAsync()
    {
        await base.ResetDatabaseAsync();
        await ResetSourceModuleDatabaseAsync();  // Reset source module's DB too
    }

    private async Task ResetSourceModuleDatabaseAsync()
    {
        var config = Services.GetRequiredService<IConfiguration>();
        var connStr = config["Modules:{SourceModule}:ConnectionStrings:Database"];
        await using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();

        _sourceRespawner ??= await Respawner.CreateAsync(conn, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["{source_schema}", "messaging"]
        });

        await _sourceRespawner.ResetAsync(conn);
    }
}
```

### Event Helpers (Source Module Commands)

```csharp
public static class {SourceModule}EventHelpers
{
    public static async Task Create{Entity}Async(
        EventConsumerTestFixture fixture,
        /* params with defaults */)
    {
        using var scope = fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var command = new {SourceCommand}(...);
        var result = await sender.Send(command);
        if (result.IsFailure)
            throw new InvalidOperationException($"Create failed: {result.Error}");
    }

    public static async Task PublishEventsFromOutboxAsync(EventConsumerTestFixture fixture)
    {
        // Trigger source module's outbox job + poll until empty
    }
}
```

### Consumer Test Pattern

```csharp
[Fact]
public async Task {SourceEvent}_Updates{Cache}()
{
    // Arrange — create entity in source module, flush events
    await {SourceModule}EventHelpers.Create{Entity}Async(Fixture);
    await {SourceModule}EventHelpers.PublishEventsFromOutboxAsync(Fixture);

    // Act — update in source module, flush events
    await {SourceModule}EventHelpers.Update{Entity}Async(Fixture, ...changed fields...);
    await {SourceModule}EventHelpers.PublishEventsFromOutboxAsync(Fixture);

    // Assert — consumer's cache reflects the update
    using var scope = Fixture.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<{ConsumerModule}DbContext>();
    var cached = await db.Set<{Cache}>()
        .FirstOrDefaultAsync(c => c.SomeField == "expected");
    Assert.NotNull(cached);
    Assert.Equal("UpdatedValue", cached.UpdatedField);
}
```

## Key Rules

- **Producer tests** use `SpyEventBus` — captures events without dispatch
- **Consumer tests** use the real `InMemoryEventBus` — events dispatch synchronously through handlers
- **Outbox flush** triggers the Quartz job then polls `messaging.outbox_messages` (max 5 seconds)
- **Cross-module consumer tests** need separate Respawner for the source module's database
- **Spy.Clear()** between create and update to isolate update events
- **LogWarning for removals** — assert log level if testing removal flows

## Checklist

- [ ] Producer fixture replaces IEventBus with SpyEventBus singleton
- [ ] Producer fixture includes "messaging" in GetSchemasToInclude
- [ ] Consumer fixture resets BOTH consumer and source module databases
- [ ] Event helpers throw on command failure (not just return)
- [ ] Outbox flush polls with timeout, doesn't just Task.Delay
- [ ] Spy.Clear() between setup and test action for update tests
