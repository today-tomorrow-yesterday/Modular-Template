using System.ComponentModel.DataAnnotations;

namespace ModularTemplate.Infrastructure.Resilience;

/// <summary>
/// Configuration options for resilience patterns (retry, circuit breaker, timeout).
/// </summary>
public sealed class ResilienceOptions : IValidatableObject
{
    /// <summary>
    /// Configuration section name for binding from appsettings.json.
    /// </summary>
    public const string SectionName = "Resilience";

    /// <summary>
    /// Gets or sets retry policy options.
    /// </summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>
    /// Gets or sets circuit breaker policy options.
    /// </summary>
    public CircuitBreakerOptions CircuitBreaker { get; set; } = new();

    /// <summary>
    /// Gets or sets timeout policy options.
    /// </summary>
    public TimeoutOptions Timeout { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Timeout.AttemptTimeoutSeconds >= Timeout.TotalTimeoutSeconds)
        {
            yield return new ValidationResult(
                "AttemptTimeoutSeconds must be less than TotalTimeoutSeconds.",
                [nameof(Timeout)]);
        }
    }
}

/// <summary>
/// Retry policy configuration options.
/// </summary>
public sealed class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    [Range(1, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in milliseconds between retries.
    /// </summary>
    [Range(100, 30000)]
    public int BaseDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to use jitter for exponential backoff.
    /// Jitter adds randomness to prevent thundering herd problems.
    /// </summary>
    public bool UseJitter { get; set; } = true;
}

/// <summary>
/// Circuit breaker policy configuration options.
/// </summary>
public sealed class CircuitBreakerOptions
{
    /// <summary>
    /// Gets or sets the sampling duration in seconds for failure rate calculation.
    /// </summary>
    [Range(1, 300)]
    public int SamplingDurationSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the failure ratio threshold (0.0 to 1.0) to open the circuit.
    /// </summary>
    [Range(0.01, 1.0)]
    public double FailureRatio { get; set; } = 0.2;

    /// <summary>
    /// Gets or sets the minimum number of requests in the sampling period
    /// before the circuit breaker evaluates the failure ratio.
    /// </summary>
    [Range(1, 100)]
    public int MinimumThroughput { get; set; } = 3;

    /// <summary>
    /// Gets or sets how long the circuit stays open before testing with a half-open state.
    /// </summary>
    [Range(1, 300)]
    public int BreakDurationSeconds { get; set; } = 30;
}

/// <summary>
/// Timeout policy configuration options.
/// </summary>
public sealed class TimeoutOptions
{
    /// <summary>
    /// Gets or sets the total timeout for an operation including all retries.
    /// </summary>
    [Range(1, 600)]
    public int TotalTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the timeout for each individual attempt.
    /// </summary>
    [Range(1, 120)]
    public int AttemptTimeoutSeconds { get; set; } = 10;
}
