namespace Rtl.Core.Infrastructure.Messaging;

/// <summary>
/// Provides retry policy utilities for message processing with exponential backoff.
/// </summary>
/// <remarks>
/// The default delays are: 5 seconds, 30 seconds, 2 minutes, 10 minutes, and 1 hour.
/// This provides a gradual backoff that prevents message storms while still allowing
/// transient failures to be retried quickly.
/// </remarks>
public static class RetryPolicy
{
    /// <summary>
    /// Default retry delays in seconds using an exponential backoff strategy.
    /// </summary>
    /// <remarks>
    /// The delays are: 5s, 30s, 2m, 10m, 1h.
    /// After the 5th retry, all subsequent retries use the maximum delay (1 hour).
    /// </remarks>
    public static readonly int[] DefaultDelaysInSeconds = [5, 30, 120, 600, 3600];

    /// <summary>
    /// Calculates the next retry time based on the retry count using exponential backoff.
    /// </summary>
    public static DateTime CalculateNextRetry(int retryCount, DateTime now)
    {
        var index = Math.Min(retryCount - 1, DefaultDelaysInSeconds.Length - 1);
        return now.AddSeconds(DefaultDelaysInSeconds[index]);
    }
}
