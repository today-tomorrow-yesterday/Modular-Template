namespace Modules.Customer.EventConsumerTests.Abstractions;

// Base class for event consumer tests.
//
// Provides an HttpClient (in-memory, no real network), fixture access for triggering
// event dispatch, and resets the database before each test.
[Collection("CustomerEventConsumer")]
public abstract class EventConsumerTestBase(EventConsumerTestFixture fixture) : IAsyncLifetime
{
    protected readonly EventConsumerTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();
    protected static int TestHomeCenterNumber => EventConsumerTestFixture.TestHomeCenterNumber;

    public async Task InitializeAsync() => await Fixture.ResetDatabaseAsync();
    public Task DisposeAsync() { Client.Dispose(); return Task.CompletedTask; }
}
