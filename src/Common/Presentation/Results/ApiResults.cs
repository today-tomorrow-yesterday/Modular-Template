using Microsoft.AspNetCore.Http;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Presentation.Results;

/// <summary>
/// Helper methods for converting Result to IResult.
/// </summary>
[Obsolete("Use ApiResponse instead. This class will be removed in a future release.")]
public static class ApiResults
{
    /// <summary>
    /// Creates a problem result from a failed Result.
    /// </summary>
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create a problem from a successful result");
        }

        return Problem(result.Error);
    }

    /// <summary>
    /// Creates a problem result from an Error.
    /// </summary>
    public static IResult Problem(Error error)
    {
        return Microsoft.AspNetCore.Http.Results.Problem(
            title: GetTitle(error),
            detail: GetDetail(error),
            type: GetType(error.Type),
            statusCode: GetStatusCode(error.Type),
            extensions: GetErrors(error));
    }

    private static string GetTitle(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Code,
            ErrorType.Problem => error.Code,
            ErrorType.NotFound => error.Code,
            ErrorType.Conflict => error.Code,
            _ => "Server failure"
        };

    private static string GetDetail(Error error) =>
        error.Type switch
        {
            ErrorType.Validation => error.Description,
            ErrorType.Problem => error.Description,
            ErrorType.NotFound => error.Description,
            ErrorType.Conflict => error.Description,
            _ => "An unexpected error occurred"
        };

    private static string GetType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Problem => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

    private static Dictionary<string, object?>? GetErrors(Error error)
    {
        if (error is not ValidationError validationError)
        {
            return null;
        }

        return new Dictionary<string, object?>
        {
            ["errors"] = validationError.Errors
        };
    }
}
