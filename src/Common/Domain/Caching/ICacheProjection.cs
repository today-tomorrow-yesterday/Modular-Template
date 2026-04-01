namespace ModularTemplate.Domain.Caching;

/// <summary>
/// Interface for cache projection entities.
/// Entities implementing this interface are subject to write protection and can only be persisted within an explicit cache write scope.
/// </summary>
public interface ICacheProjection
{
    /// <summary>
    /// The UTC timestamp when this cache record was last synchronized
    /// from the source of truth. Used for debugging data consistency issues between modules.
    /// </summary>
    DateTime LastSyncedAtUtc { get; set; }
}
