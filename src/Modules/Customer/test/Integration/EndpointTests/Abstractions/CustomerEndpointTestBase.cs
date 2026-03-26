namespace Modules.Customer.EndpointTests.Abstractions;

// Base class for Customer module endpoint tests.
//
// Handles:
// - Test lifecycle (Respawn reset)
// - HttpClient creation for in-memory API calls
[Collection("CustomerEndpoint")]
public abstract class CustomerEndpointTestBase(CustomerEndpointTestFixture fixture) : IAsyncLifetime
{
    protected readonly CustomerEndpointTestFixture Fixture = fixture;
    protected readonly HttpClient Client = fixture.CreateClient();

    protected static int TestHomeCenterNumber => CustomerEndpointTestFixture.TestHomeCenterNumber;

    // -- Lifecycle -------------------------------------------------------

    public async Task InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        Client.Dispose();
        return Task.CompletedTask;
    }
}
