using MediatR;
using ModularTemplate.Domain.Results;

namespace ModularTemplate.Application.Messaging;

/// <summary>
/// Represents a query that returns a value.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
