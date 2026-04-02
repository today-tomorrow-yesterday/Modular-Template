using MediatR;
using Microsoft.Extensions.Logging;
using ModularTemplate.Application.Exceptions;
using ModularTemplate.Domain.Results;
using System.Reflection;

namespace ModularTemplate.Application.Behaviors;

/// <summary>
/// Pipeline behavior that handles unhandled exceptions.
/// Converts <see cref="EntityNotFoundException"/> to <see cref="Result.Failure"/>
/// and wraps all other exceptions in <see cref="ModularTemplateException"/>.
/// </summary>
internal sealed class ExceptionHandlingPipelineBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (EntityNotFoundException ex)
        {
            logger.LogWarning("Entity not found for {RequestName}: {ErrorCode}",
                typeof(TRequest).Name, ex.Error.Code);

            return CreateFailureResult(ex.Error);
        }
        catch (Exception exception) when (exception is not ModularTemplateException and not OperationCanceledException)
        {
            logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);

            throw new ModularTemplateException(typeof(TRequest).Name, innerException: exception);
        }
    }

    private static TResponse CreateFailureResult(Error error)
    {
        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            Type resultType = typeof(TResponse).GetGenericArguments()[0];

            MethodInfo? failureMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m.Name == nameof(Result.Failure) && m.IsGenericMethod)
                .MakeGenericMethod(resultType);

            return (TResponse)failureMethod.Invoke(null, [error])!;
        }

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        // Fallback — should not happen if handlers follow the Result pattern
        return (TResponse)(object)Result.Failure(error);
    }
}
