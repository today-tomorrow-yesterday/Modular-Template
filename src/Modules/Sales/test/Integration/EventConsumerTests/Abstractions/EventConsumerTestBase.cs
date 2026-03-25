namespace Modules.Sales.EventConsumerTests.Abstractions;

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
