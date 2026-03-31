---
name: scaffold-integration-test
description: Use when adding integration tests for endpoints or commands - creates test fixture, test base, collection, and test class following the project's WebApplicationFactory + Respawner + Testcontainers patterns
---

# Scaffold Integration Test

Creates integration tests using the project's established patterns: WebApplicationFactory, Respawner for DB reset, xUnit collection fixtures, and progressive test arrangement.

## Test Project Structure

Each module has up to 3 test projects under `src/Modules/{Module}/test/`:

```
test/
  Application.Tests/           # Unit tests (NSubstitute mocks, no DB)
  Integration/
    Shared/                    # Shared fixture base, fakes
      {Module}TestFixtureBase.cs
      Fake{Adapter}.cs
    EndpointTests/             # HTTP endpoint tests
      Abstractions/
        {Module}EndpointTestFixture.cs
        {Module}EndpointTestBase.cs
        IntegrationTestCollection.cs
      {Feature}/
        {Test}Tests.cs
    EventProducerTests/        # Domain event → integration event tests
      Abstractions/
        EventProducerTestFixture.cs
        EventProducerTestBase.cs
        SpyEventBus.cs
    EventConsumerTests/        # Integration event → cache/command tests
      Abstractions/
        EventConsumerTestFixture.cs
        EventConsumerTestBase.cs
      {SourceModule}/
        {Event}EventFlowTests.cs
        {Event}EventHelpers.cs
```

## Shared Fixture Base

**File:** `test/Integration/Shared/{Module}TestFixtureBase.cs`

```csharp
public class {Module}TestFixtureBase : IntegrationTestFixture<Program>
{
    public const int TestHomeCenterNumber = 100;

    protected override string ResolveConnectionString(IConfiguration configuration)
        => configuration["Modules:{Module}:ConnectionStrings:Database"]
           ?? throw new InvalidOperationException("Missing {Module} connection string");

    protected override string[] GetSchemasToInclude()
        => ["{module_schema}"];  // e.g., "customers", "sales", "packages", "cache"

    // Override SeedReferenceDataAsync() if module needs post-reset seeding
}
```

## Endpoint Test Fixture

**File:** `test/Integration/EndpointTests/Abstractions/{Module}EndpointTestFixture.cs`
```csharp
public class {Module}EndpointTestFixture : {Module}TestFixtureBase;
```

Add helper methods for seeding test data (cache entries, reference data).

## Endpoint Test Base

**File:** `test/Integration/EndpointTests/Abstractions/{Module}EndpointTestBase.cs`
```csharp
[Collection("{Module}Endpoint")]
public abstract class {Module}EndpointTestBase(
    {Module}EndpointTestFixture fixture) : IAsyncLifetime
{
    protected readonly {Module}EndpointTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();
    protected static int TestHomeCenterNumber => {Module}EndpointTestFixture.TestHomeCenterNumber;

    public async Task InitializeAsync() => await Fixture.ResetDatabaseAsync();
    public Task DisposeAsync() { Client.Dispose(); return Task.CompletedTask; }
}
```

## Collection Definition

**File:** `test/Integration/EndpointTests/Abstractions/IntegrationTestCollection.cs`
```csharp
[CollectionDefinition("{Module}Endpoint")]
public sealed class {Module}EndpointTestCollection
    : ICollectionFixture<{Module}EndpointTestFixture>;
```

## Endpoint Test Pattern

```csharp
public class Get{Entity}Tests({Module}EndpointTestFixture fixture)
    : {Module}EndpointTestBase(fixture)
{
    [Fact]
    public async Task Should_Return{Entity}_When{Entity}Exists()
    {
        // Arrange — create entity via command (not direct DB insert)
        using var scope = Fixture.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var result = await sender.Send(new Create{Entity}Command(...));
        Assert.True(result.IsSuccess);

        // Look up PublicId from database (command doesn't return it)
        var db = scope.ServiceProvider.GetRequiredService<{Module}DbContext>();
        var entity = await db.Set<{Entity}>().FirstAsync(...);

        // Act
        var response = await Client.GetAsync($"/api/v1/{route}/{entity.PublicId}");
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<{Response}>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body?.Data);
        Assert.Equal(entity.PublicId, body.Data.PublicId);
    }

    [Fact]
    public async Task Should_ReturnNotFound_When{Entity}DoesNotExist()
    {
        var response = await Client.GetAsync($"/api/v1/{route}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
```

## Key Patterns

- **Arrange via commands** — never insert directly into DB, always go through the command pipeline
- **HttpAssert helpers** — use `HttpAssert.IsOkAsync(response)`, `HttpAssert.IsCreatedAsync(response)` for cleaner assertions with response body in failure messages
- **Cache seeding** — use `ICacheWriteScope.AllowWrites()` in a `using` block to bypass cache write guards
- **Progressive arrangement** — for complex tests, use helper methods that build up state incrementally (e.g., create customer → place order → add lines)
- **Respawner resets** — `ResetDatabaseAsync()` runs before each test via `IAsyncLifetime.InitializeAsync()`
- **Collection fixtures** — share the WebApplicationFactory across tests in the same collection (single app startup)

## Reference Data Seeding

For modules that need baseline data after each Respawn reset:

```csharp
protected override async Task SeedReferenceDataAsync()
{
    using var scope = Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<{Module}DbContext>();
    var cacheScope = scope.ServiceProvider.GetRequiredService<ICacheWriteScope>();

    using (cacheScope.AllowWrites())
    {
        // Seed cache entities needed by all tests
        db.Set<RetailLocationCache>().Add(
            RetailLocationCache.CreateHomeCenter(TestHomeCenterNumber, "Test HC", "TN", "37801", true));
        await db.SaveChangesAsync();
    }
}
```

## Checklist

- [ ] Fixture inherits from `IntegrationTestFixture<Program>` (not WebApplicationFactory directly)
- [ ] `GetSchemasToInclude()` returns correct schemas for Respawner
- [ ] Test base implements `IAsyncLifetime` with `ResetDatabaseAsync()` in `InitializeAsync()`
- [ ] Collection definition matches `[Collection("...")]` attribute on test base
- [ ] Arrange via commands, not direct DB inserts
- [ ] Response asserts use `ApiEnvelope<T>` deserialization
- [ ] Cache seeding uses `ICacheWriteScope.AllowWrites()`
