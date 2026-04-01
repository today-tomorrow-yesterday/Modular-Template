using Microsoft.AspNetCore.Http;
using ModularTemplate.Domain.Results;

namespace ModularTemplate.Presentation.Results;

/// <summary>
/// Static factory for creating envelope-wrapped API responses.
/// Drop-in replacement for ApiResults — supports method-group syntax in Result.Match().
/// </summary>
public static class ApiResponse
{
    /// <summary>200 OK with data.</summary>
    public static IResult Ok<T>(T data) =>
        new SuccessEnvelopeResult<T>(StatusCodes.Status200OK, data);

    /// <summary>201 Created with data and Location header.</summary>
    public static IResult Created<T>(string location, T data) =>
        new SuccessEnvelopeResult<T>(StatusCodes.Status201Created, data, location);

    /// <summary>200 OK with null data (replaces 204 NoContent).</summary>
    public static IResult Success() =>
        new SuccessEnvelopeResult<object>(StatusCodes.Status200OK, null);

    /// <summary>Error response from a domain Error. Use as method group: ApiResponse.Problem</summary>
    public static IResult Problem(Error error) =>
        new ProblemEnvelopeResult(error);

    /// <summary>Error response from a failed Result.</summary>
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create a problem from a successful result");
        }

        return Problem(result.Error);
    }
}
