using ModularTemplate.Domain.Results;

namespace ModularTemplate.Application.Exceptions;

/// <summary>
/// Exception thrown when a repository lookup expects an entity to exist but finds null.
/// Caught by <see cref="Behaviors.ExceptionHandlingPipelineBehavior{TRequest,TResponse}"/>
/// and converted to a <see cref="Result.Failure"/> with the carried <see cref="Error"/>.
/// </summary>
public sealed class EntityNotFoundException(Error error)
    : Exception($"Entity not found: [{error.Code}] {error.Description}")
{
    /// <summary>
    /// The not-found error to return through the Result pattern.
    /// </summary>
    public Error Error { get; } = error;
}
