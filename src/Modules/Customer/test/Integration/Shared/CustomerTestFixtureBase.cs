using Rtl.Core.IntegrationTests;

namespace Modules.Customer.Integration.Shared;

// Boots the full application via WebApplicationFactory and configures it for Customer integration testing.
//
// Between each test:
// - Respawn truncates the customers schema
// - No reference data seeding needed — Customer entities are created via commands during tests
//
// The StubVmfLosAdapter is already registered in DI by CustomerModule, so no fake override is needed.
// Test constants (TestHomeCenterNumber) are available for any test that needs to reference seeded data.
public abstract class CustomerTestFixtureBase : IntegrationTestFixture<Program>
{
    public const int TestHomeCenterNumber = 100;

    // ── Fixture configuration ──────────────────────────────────────

    protected override string GetConnectionString()
        => "Host=localhost;Database=customer_dev;Username=postgres;Password=postgres";

    protected override string[] GetSchemasToInclude()
        => ["customers"];
}
