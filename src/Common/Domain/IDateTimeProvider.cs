namespace Rtl.Core.Domain;

/// <summary>
/// Abstraction for getting the current date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTime UtcNow { get; }
}
