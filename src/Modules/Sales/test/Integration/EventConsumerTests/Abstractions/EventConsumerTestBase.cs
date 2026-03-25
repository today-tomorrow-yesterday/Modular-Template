namespace Modules.Sales.EventConsumerTests.Abstractions;

// Base class for event consumer tests.

// Provides an HttpClient (in-memory, no real network), fixture access for triggering
// producer commands and outbox flushes, and resets both databases before each test.
// Test classes inherit this and use helpers like CustomerEventHelpers to drive the event pipeline.
[Collection("SalesEventConsumer")]
public abstract class EventConsumerTestBase(EventConsumerTestFixture fixture) : IAsyncLifetime
{
    protected readonly EventConsumerTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();
    protected Guid TestCustomerId => Fixture.TestCustomerId;
    protected static int TestHomeCenterNumber => EventConsumerTestFixture.TestHomeCenterNumber;

    public async Task InitializeAsync() => await Fixture.ResetDatabaseAsync();
    public Task DisposeAsync() { Client.Dispose(); return Task.CompletedTask; }
}
