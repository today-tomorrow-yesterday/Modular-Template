using System.Text.Json.Serialization;
using Npgsql;

namespace ModularTemplate.Infrastructure.Persistence;

/// <summary>
/// Maps the standard AWS RDS Secrets Manager JSON payload for PostgreSQL credentials.
/// </summary>
public sealed record DatabaseSecretPayload
{
    [JsonPropertyName("engine")]
    public string Engine { get; init; } = string.Empty;

    [JsonPropertyName("host")]
    public string Host { get; init; } = string.Empty;

    [JsonPropertyName("readOnlyHost")]
    public string ReadOnlyHost { get; init; } = string.Empty;

    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; init; } = 5432;

    [JsonPropertyName("dbname")]
    public string Dbname { get; init; } = string.Empty;

    [JsonPropertyName("masterarn")]
    public string MasterArn { get; init; } = string.Empty;

    /// <summary>
    /// SSL mode resolved from configuration at startup.
    /// Set by <see cref="Secrets.DatabaseConnectionResolver"/> — not part of the AWS secret JSON.
    /// </summary>
    [JsonIgnore]
    public SslMode? SslMode { get; init; }

    /// <summary>
    /// Builds a PostgreSQL connection string from the secret payload.
    /// </summary>
    /// <param name="databaseName">Override database name; uses <see cref="Dbname"/> if null.</param>
    public string BuildConnectionString(string? databaseName = null)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Port = Port,
            Username = Username,
            Password = Password,
            Database = databaseName ?? Dbname
        };

        if (SslMode.HasValue)
        {
            builder.SslMode = SslMode.Value;
        }

        return builder.ConnectionString;
    }
}
