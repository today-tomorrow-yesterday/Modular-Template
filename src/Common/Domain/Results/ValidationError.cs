namespace Rtl.Core.Domain.Results;

/// <summary>
/// Represents a validation error containing multiple individual errors.
/// </summary>
public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base("General.Validation", "One or more validation errors occurred", ErrorType.Validation)
    {
        Errors = errors;
    }

    /// <summary>
    /// The collection of individual validation errors.
    /// </summary>
    public Error[] Errors { get; }

    /// <summary>
    /// Creates a validation error from a collection of failed results.
    /// </summary>
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new([.. results.Where(r => r.IsFailure).Select(r => r.Error)]);
}
