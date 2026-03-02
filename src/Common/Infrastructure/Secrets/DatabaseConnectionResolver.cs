using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// Resolves the database connection string at startup.
/// When AWS is enabled and a secret name is configured, fetches RDS credentials
/// from Secrets Manager and builds the connection string. Otherwise returns
/// the value from appsettings as-is.
/// </summary>
public static class DatabaseConnectionResolver
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Returns the database connection string for this host.
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        var useAws = configuration
            .GetSection(SecretProviderOptions.SectionName)
            .GetValue<bool?>("UseAws") ?? true;

        var secretName = configuration
            .GetSection(SecretProviderOptions.SectionName)
            .GetValue<string>("DatabaseSecretName");

        if (!useAws || string.IsNullOrEmpty(secretName))
        {
            return configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Database connection string is required");
        }

        using var client = new AmazonSecretsManagerClient();

        var response = client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        }).GetAwaiter().GetResult();

        var payload = JsonSerializer.Deserialize<DatabaseSecretPayload>(
            response.SecretString, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize database secret '{secretName}'.");

        var sslMode = configuration
            .GetSection(SecretProviderOptions.SectionName)
            .GetValue<string>("DatabaseSslMode");

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = payload.Host,
            Port = payload.Port,
            Username = payload.Username,
            Password = payload.Password,
            Database = payload.Dbname
        };

        if (!string.IsNullOrEmpty(sslMode))
        {
            builder.SslMode = Enum.Parse<SslMode>(sslMode, ignoreCase: true);
        }

        return builder.ConnectionString;
    }
}
