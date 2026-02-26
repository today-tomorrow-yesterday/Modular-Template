using System.Security.Cryptography;
using System.Text;

namespace Modules.Sales.Infrastructure.Seeding;

/// <summary>
/// Deterministic seed constants for the Sales module.
/// Ensures PublicIds, stock numbers, and other key values are identical across
/// database recreations and developer machines — making Swagger testing predictable.
/// </summary>
internal static class SeedConstants
{
    /// <summary>
    /// Fixed seed for Bogus Randomizer — ensures all non-key faker output (names, addresses,
    /// dollar amounts, probabilities) is identical across runs.
    /// </summary>
    public const int RandomSeed = 42;

    /// <summary>
    /// Generates a deterministic GUID from a namespace string and index.
    /// Same input always produces the same output, regardless of entity creation order.
    /// Each namespace is isolated — adding more sales won't shift package GUIDs.
    /// </summary>
    public static Guid DeterministicGuid(string ns, int index)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"seed:{ns}:{index}"));
        hash[6] = (byte)((hash[6] & 0x0F) | 0x40); // UUID version 4
        hash[8] = (byte)((hash[8] & 0x3F) | 0x80); // UUID variant 1
        return new Guid(hash.AsSpan(0, 16));
    }

    /// <summary>
    /// Overrides the PublicId property on a domain entity via reflection.
    /// Used exclusively by the seeder to stamp deterministic PublicIds without
    /// polluting domain Create() factory methods with seeding concerns.
    /// </summary>
    public static void OverridePublicId(object entity, Guid publicId)
    {
        var prop = entity.GetType().GetProperty("PublicId")
            ?? throw new InvalidOperationException(
                $"{entity.GetType().Name} does not have a PublicId property.");
        prop.SetValue(entity, publicId);
    }
}
