using Rtl.Core.Application.Caching;

namespace Rtl.Core.Infrastructure.Caching;

/// <summary>
/// Thread-safe implementation of cache write scope using AsyncLocal.
/// Allows integration event handlers to explicitly enable cache writes.
/// </summary>
public sealed class CacheWriteScope : ICacheWriteScope
{
    private static readonly AsyncLocal<bool> _isWriteAllowed = new();

    public bool IsWriteAllowed => _isWriteAllowed.Value;

    public IDisposable AllowWrites()
    {
        _isWriteAllowed.Value = true;
        return new WritePermissionScope();
    }

    private sealed class WritePermissionScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _isWriteAllowed.Value = false;
            _disposed = true;
        }
    }
}
