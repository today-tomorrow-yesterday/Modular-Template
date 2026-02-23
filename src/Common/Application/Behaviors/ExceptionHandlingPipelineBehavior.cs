using MediatR;
using Microsoft.Extensions.Logging;
using Rtl.Core.Application.Exceptions;

namespace Rtl.Core.Application.Behaviors;

/// <summary>
/// Pipeline behavior that handles unhandled exceptions.
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
        catch (Exception exception) when (exception is not RetailCoreException and not OperationCanceledException)
        {
            logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TRequest).Name);

            throw new RetailCoreException(typeof(TRequest).Name, innerException: exception);
        }
    }
}
