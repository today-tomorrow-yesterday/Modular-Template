namespace Rtl.Core.Application.Seeding;

public interface IModuleSeeder
{
    string ModuleName { get; }
    int Order { get; }
    Task SeedAsync(IServiceProvider services, CancellationToken ct = default);
}
