using Microsoft.Extensions.Configuration;
using Npgsql;
using Rtl.Core.Infrastructure.Persistence;
using System.Text.Json;

namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// Resolves the database connection string at application startup.
/// Deployed environments fetch credentials from AWS Secrets Manager.
/// Local development uses ConnectionStrings:Database from appsettings.
/// </summary>
public static class DatabaseConnectionResolver
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Returns a fully-built PostgreSQL connection string regardless of environment.
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        // Deployed enviorment: fetch credentials from AWS Secrets Manager
        if (IsAwsSecretConfigured(configuration, out var secretName))
        {
            return ResolveFromSecretsManager(configuration, secretName);
        }

        // Local dev: use connection string from appsettings
        return configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException(
                "No database connection string configured. " +
                "Set 'ConnectionStrings:Database' in appsettings or configure " +
                "'DatabaseSecret:SecretName' with 'Secrets:UseAws = true'.");
    }

    /// <summary>
    /// AWS is considered configured when Secrets:UseAws is enabled (default: true)
    /// AND DatabaseSecret:SecretName is set.
    /// </summary>
    private static bool IsAwsSecretConfigured(
        IConfiguration configuration, out string secretName)
    {
        secretName = string.Empty;

        var useAws = configuration.GetValue<bool>(
            $"{SecretProviderOptions.SectionName}:UseAws", defaultValue: true);

        if (!useAws)
            return false;

        secretName = configuration[$"{DatabaseSecretOptions.SectionName}:SecretName"] ?? string.Empty;
        return !string.IsNullOrWhiteSpace(secretName);
    }

    /// <summary>
    /// Fetches RDS credentials from Secrets Manager and builds an Npgsql connection string.
    /// SSL mode comes from DatabaseSecret:SslMode in appsettings, not from the secret itself.
    /// </summary>
    private static string ResolveFromSecretsManager(
        IConfiguration configuration, string secretName)
    {
        var secretJson = AwsSecretProvider
            .FetchSecretStringAsync(secretName)
            .GetAwaiter().GetResult();

        var credentials = JsonSerializer.Deserialize<DatabaseSecretPayload>(secretJson, JsonOptions)
            ?? throw new InvalidOperationException(
                $"AWS secret '{secretName}' returned JSON that could not be " +
                $"deserialized to {nameof(DatabaseSecretPayload)}.");

        var databaseName = configuration[$"{DatabaseSecretOptions.SectionName}:DatabaseName"];

        var sslMode = Enum.TryParse<SslMode>(
            configuration[$"{DatabaseSecretOptions.SectionName}:SslMode"], ignoreCase: true, out var parsed)
            ? parsed
            : (SslMode?)null;

        return (credentials with { SslMode = sslMode }).BuildConnectionString(databaseName);
    }
}
