using Bogus;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Rtl.Core.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected static readonly Faker Faker = new();

    private readonly IServiceScope _scope;
    protected readonly IntegrationTestWebAppFactory Factory;
    protected readonly ISender Sender;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        Factory = factory;
        _scope = factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
    }

    protected T GetService<T>() where T : notnull
        => _scope.ServiceProvider.GetRequiredService<T>();

    public async Task InitializeAsync() => await Factory.ResetDatabaseAsync();

    public Task DisposeAsync()
    {
        _scope.Dispose();
        return Task.CompletedTask;
    }
}
