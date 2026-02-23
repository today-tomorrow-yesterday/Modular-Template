using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rtl.Core.Infrastructure.Serialization;

/// <summary>
/// Provides consistent JSON serialization settings.
/// </summary>
public static class SerializerSettings
{
    /// <summary>
    /// Default serializer settings for domain events and messages.
    /// Uses TypeNameHandling.Auto with a custom binder for security.
    /// </summary>
    public static readonly JsonSerializerSettings Instance = new()
    {
        // Use Auto instead of All - only includes type info for polymorphic types
        // This is safer as it reduces attack surface for deserialization vulnerabilities
        TypeNameHandling = TypeNameHandling.Auto,
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
        // Restrict deserialization to known assemblies to prevent arbitrary type instantiation
        SerializationBinder = new SafeSerializationBinder()
    };
}

/// <summary>
/// Custom serialization binder that restricts type resolution to known safe assemblies.
/// Prevents deserialization attacks by blocking arbitrary type instantiation.
/// </summary>
internal sealed class SafeSerializationBinder : DefaultSerializationBinder
{
    private static readonly HashSet<string> AllowedAssemblyPrefixes =
    [
        "Rtl.Core.",
        "System.",
        "Microsoft."
    ];

    public override Type BindToType(string? assemblyName, string typeName)
    {
        if (assemblyName is not null &&
            !AllowedAssemblyPrefixes.Any(prefix => assemblyName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            throw new JsonSerializationException(
                $"Type '{typeName}' from assembly '{assemblyName}' is not allowed for deserialization.");
        }

        return base.BindToType(assemblyName, typeName);
    }
}
