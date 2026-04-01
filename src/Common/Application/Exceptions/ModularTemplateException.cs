using ModularTemplate.Domain.Results;

namespace ModularTemplate.Application.Exceptions;

/// <summary>
/// Base exception for application-level errors.
/// </summary>
public sealed class ModularTemplateException(
    string requestName,
    Error? error = default,
    Exception? innerException = default)
    : Exception(BuildMessage(requestName, error, innerException), innerException)
{
    private static string BuildMessage(string requestName, Error? error, Exception? innerException)
    {
        var message = $"Exception processing request '{requestName}'";
        if (error is not null)
        {
            message += $": [{error.Code}] {error.Description}";
        }
        else if (innerException is not null)
        {
            message += $": {innerException.Message}";
        }
        return message;
    }

    /// <summary>
    /// Name of the request that caused the exception.
    /// </summary>
    public string RequestName { get; } = requestName;

    /// <summary>
    /// The associated error, if any.
    /// </summary>
    public Error? Error { get; } = error;
}
