namespace Rtl.Core.Infrastructure.Secrets;

/// <summary>
/// Represents the JSON payload of an RDS-style secret stored in AWS Secrets Manager.
/// </summary>
internal sealed record DatabaseSecretPayload(
    string Host,
    int Port,
    string Username,
    string Password,
    string Dbname);
