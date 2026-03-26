namespace Modules.Customer.EventProducerTests.Abstractions;

// Base class for event producer tests.
// Provides access to the SpyEventBus for asserting on captured events.
[Collection("CustomerEventProducer")]
public abstract class EventProducerTestBase(EventProducerTestFixture fixture) : IAsyncLifetime
{
    protected readonly EventProducerTestFixture Fixture = fixture;
    protected SpyEventBus Spy => Fixture.Spy;

    public async Task InitializeAsync() => await Fixture.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;
}
