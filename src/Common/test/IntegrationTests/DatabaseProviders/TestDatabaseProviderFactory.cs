namespace Rtl.Core.IntegrationTests.DatabaseProviders;

public enum TestDatabaseType
{
    PostgreSql,
    InMemory
}

public static class TestDatabaseProviderFactory
{
    public static ITestDatabaseProvider Create(TestDatabaseType? type = null)
    {
        var providerType = type ?? GetProviderFromEnvironment();

        return providerType switch
        {
            TestDatabaseType.PostgreSql => new PostgreSqlTestDatabaseProvider(),
            TestDatabaseType.InMemory => new InMemoryTestDatabaseProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    private static TestDatabaseType GetProviderFromEnvironment()
    {
        var envValue = Environment.GetEnvironmentVariable("TEST_DB_PROVIDER");

        if (string.IsNullOrEmpty(envValue))
        {
            // Default to InMemory when no environment variable is set
            return TestDatabaseType.InMemory;
        }

        return envValue.ToLowerInvariant() switch
        {
            "postgresql" or "postgres" => TestDatabaseType.PostgreSql,
            "inmemory" or "memory" or "ef" => TestDatabaseType.InMemory,
            _ => TestDatabaseType.InMemory
        };
    }
}
