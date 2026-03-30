# Event Flow Integration Testing Strategy

> Strategy and design for end-to-end integration testing of domain events and integration events
> across modules in the Modular Monolith.

---

## Goal

Prove that the full event pipeline works: a command in a **producer module** raises a domain event,
the outbox processes it, the domain event handler publishes an integration event, the in-memory
event bus dispatches it, and the **consumer module's** integration event handler persists the
correct data to its cache tables.

We want confidence that:
1. Our domain events are raised correctly when commands execute
2. The outbox picks them up and processes them
3. Domain event handlers correctly build and publish integration events
4. Consumer modules receive those integration events and persist the right data
5. The full pipeline works end-to-end without mocks for the event flow itself

---

## Architecture Context

### Two Types of Events

**Domain Events** — raised within a single module via `Entity.Raise(new SomeDomainEvent())`.
Persisted to `messaging.outbox_messages` atomically with the business data (transactional outbox
pattern via `InsertOutboxMessagesInterceptor`). Processed by `ProcessOutboxJobBase` which invokes
domain event handlers. These handlers typically publish integration events.

**Integration Events** — cross-module messages published to `IEventBus`. Locally this uses
`InMemoryEventBus` (synchronous, in-process). In AWS environments this uses EventBridge + SQS.
Consumer modules register `IntegrationEventHandler<T>` implementations that receive these events
and typically upsert cache tables.

### Event Flow (End-to-End)

```
Producer Module                          Consumer Module
─────────────────                        ─────────────────
1. Command handler
   └─ Entity.Raise(DomainEvent)

2. SaveChanges()
   └─ InsertOutboxMessagesInterceptor
      writes to outbox_messages
      (same transaction as business data)

3. ProcessOutboxJob (Quartz timer)
   └─ Reads outbox_messages
   └─ Deserializes domain event
   └─ Invokes DomainEventHandler
      └─ Loads entity from DB
      └─ Builds IntegrationEvent
      └─ Calls IEventBus.PublishAsync()

4. IEventBus
   ├─ Dev: InMemoryEventBus
   │  └─ EventDispatcher.DispatchAsync() ──→ 5. IntegrationEventHandler
   └─ Prod: EmbEventBus → EventBridge        └─ Receives event
      → SQS → SqsPollingJob                  └─ Builds cache entity
      → EventDispatcher ─────────────────→    └─ Upserts to cache table
                                              └─ Data available for
                                                 consumer module queries
```

### Key Infrastructure Files

| Component | File |
|-----------|------|
| Domain event base | `Common/Domain/Events/DomainEvent.cs` |
| Entity.Raise() | `Common/Domain/Entities/Entity.cs` |
| Outbox interceptor | `Common/Infrastructure/Outbox/Persistence/InsertOutboxMessagesInterceptor.cs` |
| Outbox job | `Common/Infrastructure/Outbox/Job/ProcessOutboxJobBase.cs` |
| Domain event handler base | `Common/Application/Messaging/DomainEventHandler.cs` |
| Integration event base | `Common/Application/EventBus/IntegrationEvent.cs` |
| IEventBus | `Common/Application/EventBus/IEventBus.cs` |
| InMemoryEventBus | `Common/Infrastructure/EventBus/InMemory/InMemoryEventBus.cs` |
| EventDispatcher | `Common/Infrastructure/EventBus/EventDispatcher.cs` |
| Integration event handler base | `Common/Application/EventBus/IntegrationEventHandler.cs` |
| Feature flags | `Common/Infrastructure/FeatureManagement/InfrastructureFeatures.cs` |

---

## Testing Strategy

### Where Tests Live

Tests live in the **consumer module's** integration test project. The consumer owns the assertion
("did my cache get updated correctly?"). For Customer → Sales flows, tests live in
`Sales.IntegrationTests/Events/`.

The test project needs a project reference to the producer module's Application layer so it can
send commands via `ISender`.

### What We Test

Each test covers a single event flow end-to-end:

1. **Arrange** — Seed any prerequisite data the producer command needs
2. **Act** — Send the producer's command via `ISender` → flush the outbox manually
3. **Assert** — Query the consumer's database to verify the cache was updated

### How We Handle the Outbox Timer

The outbox job (`ProcessOutboxJobBase`) normally runs on a Quartz timer (every N seconds). In tests
we trigger it **manually** after the command executes — no waiting, no polling, deterministic.

Two approaches for triggering:

**Option A: Trigger via Quartz scheduler** (preferred if job is registered)
```csharp
var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();
var jobKey = new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob");
await scheduler.TriggerJob(jobKey);
await Task.Delay(500); // Brief wait for async job completion
```

**Option B: Resolve and invoke directly** (if the job class is accessible)
```csharp
var job = scope.ServiceProvider.GetRequiredService<ProcessOutboxJob>();
await job.Execute(fakeJobContext);
```

Note: Most module outbox jobs are `internal sealed class`. Option A works regardless of visibility.
Option B requires `InternalsVisibleTo` or making the job public.

### Database Strategy

Tests run against **real module databases** (e.g., `customer_dev`, `sales_dev`). This matches
what upper environments (QA, ITG, PRO) will use — same connection strings, just different hosts.

**Respawner** cleans both databases between tests:
- Sales database (`sales_dev`): schemas `sales`, `packages`, `cache`
- Customer database (`customer_dev`): schema `customers`
- Both: `messaging` schema tables (outbox/inbox messages)

The `SalesTestFactory` manages both Respawner connections.

### Feature Flags

The outbox/inbox jobs check feature flags before processing. The test factory must enable them:

```csharp
builder.UseSetting("Features:Infrastructure:Outbox", "true");
builder.UseSetting("Features:Infrastructure:Inbox", "true");
```

### In-Memory vs Real Event Bus

Locally and in integration tests, `InMemoryEventBus` is used. This dispatches events
**synchronously** — when the domain event handler calls `eventBus.PublishAsync()`, the
`EventDispatcher` immediately invokes all registered `IntegrationEventHandler<T>` implementations
in the same process. No SQS, no EventBridge.

In upper environments (QA, ITG, PRO), the real `EmbEventBus` → EventBridge → SQS pipeline is used.
The same integration tests can run there — the only difference is the event bus implementation
and the connection strings.

---

## Event Flows to Test

### Priority 1: Customer → Sales (Foundation)

These are the cache tables that the Sales module's package tests already depend on.

| Producer Command | Domain Event | Integration Event | Sales Handler | Cache Table |
|-----------------|--------------|-------------------|---------------|-------------|
| `SyncCustomerFromCrmCommand` | `CustomerCreatedDomainEvent` | `CustomerCreatedIntegrationEvent` | `CustomerCreatedIntegrationEventHandler` | `cache.customers` |
| Customer update commands | Various (`CustomerNameChanged`, etc.) | Corresponding integration events | 9 update handlers | `cache.customers` |

**First test to write:** `SyncCustomerFromCrmCommand` → verify `cache.customers` in `sales_dev`.

#### SyncCustomerFromCrmCommand Shape

```csharp
new SyncCustomerFromCrmCommand(
    CrmCustomerId: 42,
    HomeCenterNumber: 100,
    LifecycleStage: LifecycleStage.Lead,  // Customer module's enum
    FirstName: "John",
    MiddleName: null,
    LastName: "Doe",
    NameExtension: null,
    DateOfBirth: null,
    SalesAssignments: [],                  // Optional — maps to SalesPerson entities
    ContactPoints: [
        new SyncContactPointDto(ContactPointType.Email, "john@test.com", IsPrimary: true),
        new SyncContactPointDto(ContactPointType.Phone, "555-1234", IsPrimary: false)
    ],
    Identifiers: [
        new SyncIdentifierDto(IdentifierType.CrmCustomerId, "42")
    ],
    MailingAddress: null,
    SalesforceUrl: null,
    CreatedOn: null,
    LastModifiedOn: null);
```

#### What the Sales Handler Does

`CustomerCreatedIntegrationEventHandler` in Sales module:
1. Extracts display name: `"{FirstName} {LastName}".Trim()`
2. Extracts SalesforceAccountId from Identifiers
3. Extracts Email/Phone from ContactPoints
4. Extracts Primary/Secondary sales person from SalesAssignments
5. Builds `CustomerCache` entity
6. Sends `UpsertCustomerCacheCommand` → persists to `cache.customers`

#### Assertions

```csharp
var cachedCustomer = await salesDb.Set<CustomerCache>()
    .FirstOrDefaultAsync(c => c.HomeCenterNumber == 100 &&
                              c.FirstName == "John" &&
                              c.LastName == "Doe");

Assert.NotNull(cachedCustomer);
Assert.Equal("Lead", cachedCustomer.LifecycleStage.ToString());
Assert.Equal("John Doe", cachedCustomer.DisplayName);
Assert.Equal(100, cachedCustomer.HomeCenterNumber);
Assert.Equal("john@test.com", cachedCustomer.Email);
```

### Priority 2: Organization → Sales

| Producer Command | Integration Event | Sales Handler | Cache Table |
|-----------------|-------------------|---------------|-------------|
| HomeCenterChanged | `HomeCenterChangedIntegrationEvent` | `HomeCenterChangedIntegrationEventHandler` | `cache.retail_location_cache` |
| UserAccessGranted | `UserAccessGrantedIntegrationEvent` | `UserAccessGrantedIntegrationEventHandler` | `cache.authorized_users_cache` |

### Priority 3: Inventory → Sales

| Producer Command | Integration Event | Sales Handler | Cache Table |
|-----------------|-------------------|---------------|-------------|
| OnLotHomeAdded | `OnLotHomeAddedToInventoryIntegrationEvent` | Handler | `cache.on_lot_homes_cache` |
| LandParcelAdded | `LandParcelAddedToInventoryIntegrationEvent` | Handler | `cache.land_parcels_cache` |

### Priority 4: Funding → Sales

| Producer Command | Integration Event | Sales Handler | Cache Table |
|-----------------|-------------------|---------------|-------------|
| FundingRequestSubmitted | `FundingRequestSubmittedIntegrationEvent` | Handler | `cache.funding_requests_cache` |

---

## Known Blocker: Outbox Deserialization Bug

**Status:** Pre-existing bug discovered during testing. Blocks E2E event tests.

**Problem:** `InsertOutboxMessagesInterceptor` stores domain events with short type name:
```csharp
Type = domainEvent.GetType().Name  // e.g., "CustomerCreatedDomainEvent"
```

But `ProcessOutboxJobBase.Execute()` tries to resolve the type with:
```csharp
var domainEventType = Type.GetType(outboxMessage.Type)!;  // returns null for short names
```

`Type.GetType("CustomerCreatedDomainEvent")` returns `null` because it's not assembly-qualified.
The JSON then deserializes to a `JObject` which can't be cast to `IDomainEvent`.

**Error:** `System.InvalidCastException: Unable to cast object of type 'Newtonsoft.Json.Linq.JObject' to type 'Rtl.Core.Domain.Events.IDomainEvent'.`

**Evidence:** Found in `customer_dev.messaging.outbox_messages` — messages have `retry_count > 0`
and the cast exception in the `error` column.

**Fix Options:**
1. Change interceptor to store `AssemblyQualifiedName` instead of `.Name` (one-line fix, affects all modules)
2. Add type resolution logic in `ProcessOutboxJobBase` that scans the `HandlersAssembly` to find the type by short name
3. Store both short name and assembly-qualified name

**Recommended:** Option 2 — add type resolution in `ProcessOutboxJobBase` that scans the
`HandlersAssembly` to find the type by short name when `Type.GetType()` returns null.
This is backwards-compatible with existing outbox messages already stored with short names,
and doesn't require re-processing or data migration. Option 1 (AssemblyQualifiedName) would
only fix NEW messages — existing unprocessed messages with short names would still fail.

```csharp
// In ProcessOutboxJobBase.Execute(), after line 97:
var domainEventType = Type.GetType(outboxMessage.Type)
    ?? HandlersAssembly.GetTypes()
        .FirstOrDefault(t => t.Name == outboxMessage.Type && typeof(IDomainEvent).IsAssignableFrom(t));
```

**Workaround for tests (until bug is fixed):** Skip the outbox and directly publish the
integration event to `IEventBus` to test the consumer handler in isolation. This tests step 5
of the pipeline (consumer handler → cache) but not steps 1-4 (command → outbox → domain handler
→ publish). The domain events are raised in the Application assembly, not the HandlersAssembly,
so the scan may also need to check the Application assembly.

---

## Test Infrastructure Changes Required

### Project References

`Sales.IntegrationTests.csproj` needs:
```xml
<ProjectReference Include="..\..\..\Customer\Infrastructure\Modules.Customer.Infrastructure.csproj" />
```

This gives access to `SyncCustomerFromCrmCommand` and Customer domain enums.

### SalesTestFactory Updates

```csharp
// Enable outbox/inbox processing
builder.UseSetting("Features:Infrastructure:Outbox", "true");
builder.UseSetting("Features:Infrastructure:Inbox", "true");

// Add second Respawner for customer_dev
private const string CustomerDbConnectionString =
    "Host=localhost;Database=customer_dev;Username=postgres;Password=postgres";

public new async Task ResetDatabaseAsync()
{
    await base.ResetDatabaseAsync();        // sales_dev
    await ResetCustomerDatabaseAsync();      // customer_dev
}
```

### OutboxHelper

Utility to manually trigger outbox processing:

```csharp
internal static class OutboxHelper
{
    public static async Task FlushCustomerOutboxAsync(IServiceProvider services)
    {
        var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();
        var jobKey = new JobKey("Modules.Customer.Infrastructure.Outbox.ProcessOutboxJob");
        await scheduler.TriggerJob(jobKey);
        await Task.Delay(500);
    }
}
```

### Test File Location

```
Sales.IntegrationTests/
  Events/
    CustomerCreatedEventFlowTests.cs     ← Priority 1
    CustomerUpdatedEventFlowTests.cs     ← Priority 1 (9 update events)
    HomeCenterChangedEventFlowTests.cs   ← Priority 2
    UserAccessGrantedEventFlowTests.cs   ← Priority 2
    InventoryEventFlowTests.cs           ← Priority 3
    FundingEventFlowTests.cs             ← Priority 4
```

---

## Test Pattern (Template)

```csharp
[Collection("SalesIntegration")]
public class CustomerCreatedEventFlowTests : IAsyncLifetime
{
    private readonly SalesTestFactory _factory;
    private readonly IServiceScope _scope;
    private readonly ISender _sender;

    public CustomerCreatedEventFlowTests(SalesTestFactory factory)
    {
        _factory = factory;
        _scope = factory.Services.CreateScope();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
    }

    public async Task InitializeAsync() => await _factory.ResetDatabaseAsync();
    public Task DisposeAsync() { _scope.Dispose(); return Task.CompletedTask; }

    [Fact]
    public async Task CustomerCreated_ShouldPopulateSalesCustomerCache()
    {
        // 1. Send command to Customer module
        var command = new SyncCustomerFromCrmCommand(...);
        var result = await _sender.Send(command);
        Assert.True(result.IsSuccess);

        // 2. Flush Customer outbox → triggers domain event handler
        //    → publishes integration event → InMemoryEventBus dispatches
        //    → Sales handler upserts cache
        await OutboxHelper.FlushCustomerOutboxAsync(_factory.Services);

        // 3. Verify Sales cache was updated
        var salesDb = _scope.ServiceProvider.GetRequiredService<SalesDbContext>();
        var cached = await salesDb.Set<CustomerCache>()
            .FirstOrDefaultAsync(c => c.FirstName == "John" && c.LastName == "Doe");

        Assert.NotNull(cached);
        Assert.Equal("John Doe", cached.DisplayName);
        Assert.Equal(100, cached.HomeCenterNumber);
    }
}
```

---

## Environment Portability

| Environment | Event Bus | Database | Notes |
|-------------|-----------|----------|-------|
| Local dev | InMemoryEventBus | `*_dev` PostgreSQL | Synchronous dispatch, fast |
| CI pipeline | InMemoryEventBus | Pipeline PostgreSQL | Same as local, different connection strings |
| QA/ITG | EmbEventBus → EventBridge → SQS | Environment PostgreSQL | Real AWS infrastructure |
| Production | EmbEventBus → EventBridge → SQS | Production PostgreSQL | Full resilience (retry, circuit breaker, DLQ) |

The same test code works in all environments. The only differences are:
- Connection strings (via config/env vars)
- Event bus implementation (auto-selected by `IHostEnvironment`)
- In upper environments, the outbox flush may need longer delays or polling since events are async

---

## Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Test location | Consumer module's test project | Consumer owns the assertion |
| Outbox handling | Manual trigger via Quartz scheduler | Fast, deterministic, no timer waits. Works even with `internal` job classes. |
| Database | Real module databases (`*_dev`) | Matches upper environments, portable to CI |
| Cleanup | Respawner on both DBs | Full isolation between tests |
| Event bus | InMemoryEventBus (default for dev) | Synchronous, no external deps |
| First test | Customer Created → Sales cache | Foundation for all other cache tests |
| iSeries | Mocked via `FakeiSeriesAdapter` in DI | Not relevant to event flow, keeps tests focused |

---

## Gotchas & Lessons Learned

These were discovered during the initial implementation and cost significant debugging time.
Read these before implementing.

### 1. Sales Module Requires PostgreSQL (NOT InMemory)

The Sales module uses JSONB columns with custom `VersionedJsonConverter<T>` value converters.
EF Core InMemory provider cannot handle `JsonDocument` types. The test factory MUST force
PostgreSQL via:
```csharp
static SalesTestFactory() =>
    Environment.SetEnvironmentVariable("TEST_DB_PROVIDER", "postgresql");
```

### 2. Each Module Has Its Own Database

Connection strings are module-specific under `Modules:{Name}:ConnectionStrings:Database` in
`appsettings.Development.json`. The monolith host's `Program.cs` reads these per-module.
The `WebApplicationFactory` picks them up automatically — no need to override unless using a
different test database.

| Module | Database | Connection String Key |
|--------|----------|----------------------|
| Sales | `sales_dev` | `Modules:Sales:ConnectionStrings:Database` |
| Customer | `customer_dev` | `Modules:Customer:ConnectionStrings:Database` |
| Organization | `organization_dev` | `Modules:Organization:ConnectionStrings:Database` |
| Inventory | `inventory_dev` | `Modules:Inventory:ConnectionStrings:Database` |
| Funding | `funding_dev` | `Modules:Funding:ConnectionStrings:Database` |

### 3. Collection Name Collision

The Common test project defines `IntegrationTestCollection` which uses `IntegrationTestWebAppFactory`.
If the Sales test project also defines a collection with the same name, xUnit v2 resolves by
string and picks the Common project's version — meaning your custom `SalesTestFactory` never
gets created. Use a unique name:
```csharp
[CollectionDefinition("SalesIntegration")]
public sealed class SalesIntegrationTestCollection : ICollectionFixture<SalesTestFactory>;
```

### 4. Encryption Key Must Match the Database

The base `IntegrationTestWebAppFactory` sets a test encryption key. If tests run against a
database with data encrypted by a different key (e.g., `sales_dev` seeded with the dev key from
`launchSettings.json`), decryption fails with `AuthenticationTagMismatchException`. Either:
- Use the same key as the database (`MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=` for dev)
- Or ensure all test data is freshly created within the test (encrypted with the test key)

### 5. EmbProducerOptions Validation

`WebApplicationFactory` boots the full host which validates messaging config on startup.
`EmbProducerOptions.EventBus` is `[Required]` but empty in base `appsettings.json`. The factory
must provide it:
```csharp
builder.UseSetting("Messaging:EmbProducer:EventBus", "test-event-bus");
builder.UseSetting("Messaging:SqsConsumer:SqsQueueUrl", "https://sqs.us-east-1.amazonaws.com/000000000000/test-queue");
```
This was added to the shared `IntegrationTestWebAppFactory.ConfigureWebHost()` so all modules
benefit.

### 6. `sale_number_seq` Sequence Not in Migration

The `SaleNumberGenerator` uses a PostgreSQL sequence `sales.sale_number_seq` that is NOT created
by the EF migration. It's created by the seeder. If seeding is disabled, the sequence must be
created manually or the test factory must create it:
```sql
CREATE SEQUENCE IF NOT EXISTS sales.sale_number_seq START WITH 100001;
```

### 7. Cache Entities Need ICacheWriteScope

Any insert/update to a table backed by an entity implementing `ICacheProjection` (CustomerCache,
AuthorizedUserCache, RetailLocationCache, etc.) must be wrapped in `cacheScope.AllowWrites()`.
The `CacheWriteGuardInterceptor` throws if you don't. This applies to both test seed data and
production integration event handlers.

### 8. WebApplicationFactory Does NOT Run Program.cs Pipeline

`WebApplicationFactory` creates the host but does NOT execute the middleware pipeline
(`ApplyMigrations`, `SeedDataAsync`, etc.). These run after `app.Build()` in `Program.cs` but
the factory intercepts before that. Migrations and seeding must be handled separately in tests.
