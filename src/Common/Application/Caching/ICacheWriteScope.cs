namespace Rtl.Core.Application.Caching;

/// <summary>
/// Provides scoped write permission for cache projection entities.
/// Only code within an active write scope can persist cache entities.
/// </summary>
public interface ICacheWriteScope
{
    /// <summary>
    /// Gets a value indicating whether cache writes are currently allowed.
    /// </summary>
    bool IsWriteAllowed { get; }

    /// <summary>
    /// Opens a write permission scope. Cache writes are allowed until the returned
    /// IDisposable is disposed.
    /// </summary>
    /// <returns>A disposable that revokes write permission when disposed.</returns>
    IDisposable AllowWrites();
}
