using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Rtl.Core.Application.EventBus;

namespace Rtl.Core.Infrastructure.Resilience;

/// <summary>
/// Resilience wrapper that sits above <see cref="Rtl.Core.Infrastructure.EventBus.Emb.EmbEventBus"/>
/// (or any <see cref="IEventBus"/> implementation) and adds Polly-based fault tolerance.
/// </summary>
/// <remarks>
/// <para>
/// This is a decorator — it does not publish events itself. It wraps the inner
/// <see cref="IEventBus"/> (currently <see cref="Rtl.Core.Infrastructure.EventBus.Emb.EmbEventBus"/>)
/// with a Polly resilience pipeline so that every call to PublishAsync is protected by:
/// </para>
/// <list type="number">
///   <item>Total timeout — bounds the entire operation including all retries</item>
///   <item>Retry with exponential backoff and optional jitter</item>
///   <item>Circuit breaker — fails fast when the downstream service is unavailable</item>
///   <item>Per-attempt timeout — bounds each individual publish attempt</item>
/// </list>
/// <para>
/// Call chain: IEventBus (this wrapper) → EmbEventBus → IEMBProducer → AWS EventBridge
/// </para>
/// <para>
/// Configuration is provided via <see cref="ResilienceOptions"/> in appsettings (section: "Resilience").
/// </para>
/// </remarks>
internal sealed class ResilientEventBusWrapper : IEventBus
{
    private readonly IEventBus _innerEventBus;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ILogger<ResilientEventBusWrapper> _logger;

    public ResilientEventBusWrapper(
        IEventBus innerEventBus,
        IOptions<ResilienceOptions> options,
        ILogger<ResilientEventBusWrapper> logger)
    {
        _innerEventBus = innerEventBus;
        _logger = logger;

        var resilienceOptions = options.Value;

        // Pipeline order (outermost to innermost):
        // 1. Total timeout - caps the entire operation including retries
        // 2. Retry - retries failed attempts with backoff
        // 3. Circuit breaker - fails fast when error threshold exceeded
        // 4. Attempt timeout - caps each individual attempt
        _resiliencePipeline = new ResiliencePipelineBuilder()
            .AddTimeout(CreateTotalTimeoutOptions(resilienceOptions.Timeout))
            .AddRetry(CreateRetryOptions(resilienceOptions.Retry))
            .AddCircuitBreaker(CreateCircuitBreakerOptions(resilienceOptions.CircuitBreaker))
            .AddTimeout(CreateAttemptTimeoutOptions(resilienceOptions.Timeout))
            .Build();
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : IIntegrationEvent
    {
        await _resiliencePipeline.ExecuteAsync(
            async ct => await _innerEventBus.PublishAsync(integrationEvent, ct),
            cancellationToken);
    }

    private RetryStrategyOptions CreateRetryOptions(RetryOptions options)
    {
        return new RetryStrategyOptions
        {
            MaxRetryAttempts = options.MaxRetryAttempts,
            Delay = TimeSpan.FromMilliseconds(options.BaseDelayMilliseconds),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = options.UseJitter,
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "Retry attempt {AttemptNumber}/{MaxAttempts} for EventBus publish after {Delay}ms. Exception: {ExceptionMessage}",
                    args.AttemptNumber,
                    options.MaxRetryAttempts,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome.Exception?.Message ?? "No exception");
                return ValueTask.CompletedTask;
            }
        };
    }

    private CircuitBreakerStrategyOptions CreateCircuitBreakerOptions(CircuitBreakerOptions options)
    {
        return new CircuitBreakerStrategyOptions
        {
            SamplingDuration = TimeSpan.FromSeconds(options.SamplingDurationSeconds),
            FailureRatio = options.FailureRatio,
            MinimumThroughput = options.MinimumThroughput,
            BreakDuration = TimeSpan.FromSeconds(options.BreakDurationSeconds),
            OnOpened = args =>
            {
                _logger.LogError(
                    "Circuit breaker OPENED for EventBus. Break duration: {BreakDuration}s. Failure ratio exceeded threshold. Exception: {ExceptionMessage}",
                    options.BreakDurationSeconds,
                    args.Outcome.Exception?.Message ?? "No exception");
                return ValueTask.CompletedTask;
            },
            OnClosed = _ =>
            {
                _logger.LogInformation(
                    "Circuit breaker CLOSED for EventBus. Resuming normal operations");
                return ValueTask.CompletedTask;
            },
            OnHalfOpened = _ =>
            {
                _logger.LogInformation(
                    "Circuit breaker HALF-OPEN for EventBus. Testing with next request");
                return ValueTask.CompletedTask;
            }
        };
    }

    private TimeoutStrategyOptions CreateTotalTimeoutOptions(TimeoutOptions options)
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(options.TotalTimeoutSeconds),
            OnTimeout = args =>
            {
                _logger.LogError(
                    "Total timeout exceeded for EventBus publish after {Timeout}s. Operation cancelled",
                    options.TotalTimeoutSeconds);
                return ValueTask.CompletedTask;
            }
        };
    }

    private TimeoutStrategyOptions CreateAttemptTimeoutOptions(TimeoutOptions options)
    {
        return new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(options.AttemptTimeoutSeconds),
            OnTimeout = args =>
            {
                _logger.LogWarning(
                    "Attempt timeout exceeded for EventBus publish after {Timeout}s. Will retry if attempts remaining",
                    options.AttemptTimeoutSeconds);
                return ValueTask.CompletedTask;
            }
        };
    }
}
