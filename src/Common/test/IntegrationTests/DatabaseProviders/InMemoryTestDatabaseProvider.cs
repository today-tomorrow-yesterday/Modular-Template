using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Rtl.Core.IntegrationTests.DatabaseProviders;

public sealed class InMemoryTestDatabaseProvider : ITestDatabaseProvider
{
    private readonly string _databaseName = $"TestDb_{Guid.NewGuid():N}";
    private IServiceProvider? _serviceProvider;
    private readonly List<Type> _dbContextTypes = [];

    public string ProviderName => "EF Core InMemory";

    public void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Use Testing environment to skip relational migrations
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:Cache", "localhost:6379");
        builder.UseSetting("Testing:UseInMemoryDatabase", "true");
        builder.UseSetting("Testing:InMemoryDatabaseName", _databaseName);

        builder.ConfigureTestServices(services =>
        {
            // Find all registered DbContext types
            var dbContextDescriptors = services
                .Where(d => d.ServiceType.IsAssignableTo(typeof(DbContext)) && !d.ServiceType.IsAbstract)
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                _dbContextTypes.Add(descriptor.ServiceType);
            }

            // Find DbContextOptions registrations and replace with InMemory
            var optionsDescriptors = services
                .Where(d => d.ServiceType.IsGenericType &&
                           d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>))
                .ToList();

            foreach (var descriptor in optionsDescriptors)
            {
                var contextType = descriptor.ServiceType.GetGenericArguments()[0];

                // Remove existing registrations for this context
                services.RemoveAll(descriptor.ServiceType);
                services.RemoveAll(contextType);

                // Register DbContextOptions<T> with InMemory
                RegisterInMemoryDbContext(services, contextType);
            }
        });
    }

    private void RegisterInMemoryDbContext(IServiceCollection services, Type contextType)
    {
        // Use the non-generic AddDbContext approach with factory
        var optionsType = typeof(DbContextOptions<>).MakeGenericType(contextType);

        // Create options using the standard EF Core builder
        services.AddScoped(optionsType, sp =>
        {
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder = (DbContextOptionsBuilder)Activator.CreateInstance(optionsBuilderType)!;

            optionsBuilder.UseInMemoryDatabase(_databaseName);

            return optionsBuilder.Options;
        });

        // Register the DbContext itself
        services.AddScoped(contextType, sp =>
        {
            var options = sp.GetRequiredService(optionsType);
            return Activator.CreateInstance(contextType, options)!;
        });
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task ResetAsync()
    {
        if (_serviceProvider is null) return;

        using var scope = _serviceProvider.CreateScope();

        // Reset each registered DbContext
        foreach (var contextType in _dbContextTypes)
        {
            try
            {
                var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
                await context.Database.EnsureDeletedAsync();
                await context.Database.EnsureCreatedAsync();
            }
            catch
            {
                // Context may not be available in InMemory mode
            }
        }
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
