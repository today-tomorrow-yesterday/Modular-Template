using MediatR;
using Microsoft.Extensions.Logging;
using ModularTemplate.Domain.Results;
using System.Diagnostics;

namespace ModularTemplate.Application.Behaviors;

/// <summary>
/// Pipeline behavior that logs request processing.
/// </summary>
internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string moduleName = GetModuleName(typeof(TRequest).FullName!);
        string requestName = typeof(TRequest).Name;

        Activity.Current?.SetTag("request.module", moduleName);
        Activity.Current?.SetTag("request.name", requestName);

        logger.LogInformation("Processing request {RequestName} in module {ModuleName}", requestName, moduleName);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed request {RequestName} successfully", requestName);
        }
        else
        {
            logger.LogWarning(
                "Completed request {RequestName} with error: {ErrorCode} - {ErrorDescription}",
                requestName,
                result.Error.Code,
                result.Error.Description);
        }

        return result;
    }

    private static string GetModuleName(string requestName)
    {
        var parts = requestName.Split('.');
        return parts.Length > 2 ? parts[2] : "Unknown";
    }
}
