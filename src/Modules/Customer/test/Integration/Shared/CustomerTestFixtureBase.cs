using Microsoft.Extensions.Configuration;
using Rtl.Core.IntegrationTests;

namespace Modules.Customer.Integration.Shared;

// Boots the full application via WebApplicationFactory and configures it for Customer integration testing.
//
// Between each test:
// - Respawn truncates the customers schema
// - No reference data seeding needed — Customer entities are created via commands during tests
//
// Connection string comes from the app's config pipeline:
//   Modules:Customer:ConnectionStrings:Database -> ConnectionStrings:Database fallback
public abstract class CustomerTestFixtureBase : IntegrationTestFixture<Program>
{
    public const int TestHomeCenterNumber = 100;

    protected override string ResolveConnectionString(IConfiguration configuration)
        => configuration["Modules:Customer:ConnectionStrings:Database"]
           ?? configuration.GetConnectionString("Database")
           ?? throw new InvalidOperationException(
               "No Customer database connection string found in configuration.");

    protected override string[] GetSchemasToInclude()
        => ["customers"];
}
