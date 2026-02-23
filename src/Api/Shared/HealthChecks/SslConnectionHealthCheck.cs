using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace Rtl.Core.Api.Shared.HealthChecks;

/// <summary>
/// Verifies that the database connection is using SSL/TLS encryption.
/// </summary>
public class SslConnectionHealthCheck(string connectionString) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Parse configuration to determine intent
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var sslMode = builder.SslMode;
            var isSecureConfig = sslMode == SslMode.Require || sslMode == SslMode.VerifyCA || sslMode == SslMode.VerifyFull;
            var isLocalhost = connectionString.Contains("localhost") || connectionString.Contains("127.0.0.1");

            // In Production (non-localhost), we DEMAND secure configuration.
            // If the connection opened successfully with SslMode=Require+, we know it is secure.
            if (!isSecureConfig && !isLocalhost)
            {
                return HealthCheckResult.Unhealthy(
                    $"Database connection is configured with SslMode='{sslMode}'. This is a compliance violation for remote connections.");
            }

            return HealthCheckResult.Healthy(
                $"Database connection established. SslMode: {sslMode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                $"SSL connection health check failed: {ex.Message}", ex);
        }
    }
}
