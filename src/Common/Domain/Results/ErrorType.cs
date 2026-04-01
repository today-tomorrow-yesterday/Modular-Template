namespace ModularTemplate.Domain.Results;

/// <summary>
/// Represents the type of error.
/// </summary>
public enum ErrorType
{
    /// <summary>
    /// Generic failure error.
    /// </summary>
    Failure = 0,

    /// <summary>
    /// Validation error.
    /// </summary>
    Validation = 1,

    /// <summary>
    /// Business logic problem.
    /// </summary>
    Problem = 2,

    /// <summary>
    /// Resource not found.
    /// </summary>
    NotFound = 3,

    /// <summary>
    /// Conflict with existing state.
    /// </summary>
    Conflict = 4
}
