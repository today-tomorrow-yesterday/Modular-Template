using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.Application;

/// <summary>
/// Centralized application identity configuration.
/// </summary>
/// <remarks>
/// <para>
/// This configuration section defines the application's identity across all environments.
/// When bootstrapping this template for a new project, update these values and the
/// related configuration values will follow the naming conventions below.
/// </para>
/// <para>
/// <b>Naming Convention Guide:</b>
/// <list type="bullet">
///   <item><c>Name</c>: PascalCase with dot separator (e.g., "Acme.Orders") - used for namespaces</item>
///   <item><c>DisplayName</c>: Human-readable name (e.g., "Acme Orders API") - used for UI/Swagger</item>
///   <item><c>ShortName</c>: kebab-case (e.g., "acme-orders") - used for resource naming</item>
/// </list>
/// </para>
/// <para>
/// <b>Derived Values (update in appsettings.json):</b>
/// <list type="bullet">
///   <item>Database name: lowercase, no separators (e.g., "acmeorders")</item>
///   <item>EventBusName: {ShortName}-events (e.g., "acme-orders-events")</item>
///   <item>EventSource: dot notation (e.g., "acme.orders")</item>
///   <item>Auth Audience: {ShortName}-api (e.g., "acme-orders-api")</item>
/// </list>
/// </para>
/// </remarks>
public sealed class ApplicationOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Application";

    /// <summary>
    /// Gets the application name in PascalCase with dot separator.
    /// Used for namespaces and assembly names.
    /// </summary>
    /// <example>Acme.Orders, Rtl.Core, MyCompany.Inventory</example>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the human-readable display name.
    /// Used for API documentation, Swagger UI, and user-facing text.
    /// </summary>
    /// <example>Acme Orders API, Retail Core API</example>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the short name in kebab-case.
    /// Used for AWS resources, database schemas, and external identifiers.
    /// </summary>
    /// <example>acme-orders, retail-core</example>
    public required string ShortName { get; init; }

    /// <summary>
    /// Gets the database name.
    /// If not explicitly set, derived from <see cref="Name"/> (lowercase, no dots).
    /// </summary>
    /// <example>acmeorders, retailcore</example>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of module schemas this application manages.
    /// For single-module deployments, leave empty to auto-derive from <see cref="DatabaseName"/>.
    /// For the monolith, list all module schemas.
    /// </summary>
    /// <example>["sales"], ["sample", "orders", "organization", "customer", "sales"]</example>
    public string[] Modules { get; set; } = [];

    /// <summary>
    /// Gets the derived database name, computing it from Name if not explicitly set.
    /// </summary>
    public string GetDatabaseName() =>
        string.IsNullOrEmpty(DatabaseName)
            ? Name.Replace(".", "").ToLowerInvariant()
            : DatabaseName;

    /// <summary>
    /// Gets the module schemas, defaulting to DatabaseName if not explicitly configured.
    /// </summary>
    public string[] GetModules() =>
        Modules.Length > 0
            ? Modules
            : [GetDatabaseName()];

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            yield return new ValidationResult("Name is required.", [nameof(Name)]);
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            yield return new ValidationResult("DisplayName is required.", [nameof(DisplayName)]);
        }

        if (string.IsNullOrWhiteSpace(ShortName))
        {
            yield return new ValidationResult("ShortName is required.", [nameof(ShortName)]);
        }
    }
}
