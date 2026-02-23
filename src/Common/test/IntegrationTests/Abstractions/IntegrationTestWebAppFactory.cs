using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Rtl.Core.IntegrationTests.DatabaseProviders;
using Xunit;

namespace Rtl.Core.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly ITestDatabaseProvider _databaseProvider;

    public IntegrationTestWebAppFactory()
    {
        _databaseProvider = TestDatabaseProviderFactory.Create();
        Console.WriteLine($"[IntegrationTests] Using database provider: {_databaseProvider.ProviderName}");
    }

    public string DatabaseProviderName => _databaseProvider.ProviderName;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _databaseProvider.ConfigureWebHost(builder);

        const string testKey = "8LQDTJ33CPbaageGk/STuqnge2ZJd/Q+rwvEGbE1X7E=";
        builder.UseSetting("Encryption:Key", testKey);
        Environment.SetEnvironmentVariable("ENCRYPTION_KEY", testKey);

        builder.ConfigureTestServices(services =>
        {
            // Common test service overrides can go here
        });
    }

    public async Task InitializeAsync()
    {
        await _databaseProvider.InitializeAsync();

        // Trigger app creation to run migrations
        _ = Services;

        // Pass service provider for reset operations
        _databaseProvider.SetServiceProvider(Services);
    }

    public async Task ResetDatabaseAsync() => await _databaseProvider.ResetAsync();

    public new async Task DisposeAsync()
    {
        await _databaseProvider.DisposeAsync();
        await base.DisposeAsync();
    }
}
